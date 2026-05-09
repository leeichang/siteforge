using SiteForge.Core.DTOs;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;
using SiteForge.Core.Interfaces.Services;
using SiteForge.Core.Utilities;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SiteForge.Core.Services;

public class AiConversationServiceImpl : AiConversationService
{
    private readonly RAiConversationRepository _conversations;
    private readonly RAiMessageRepository _messages;
    private readonly RSiteRepository _sites;
    private readonly RPageRepository _pages;

    public AiConversationServiceImpl(
        RAiConversationRepository conversations,
        RAiMessageRepository messages,
        RSiteRepository sites,
        RPageRepository pages)
    {
        _conversations = conversations;
        _messages = messages;
        _sites = sites;
        _pages = pages;
    }

    public async Task<List<ConversationDto>> GetBySiteAsync(Guid siteId) =>
        (await _conversations.GetBySiteIdAsync(siteId)).Select(Mappers.ToDto).ToList();

    public async Task<ConversationDto> CreateAsync(CreateConversationRequest request)
    {
        var conversation = await _conversations.AddAsync(new AiConversation
        {
            SiteId = request.SiteId,
            PageId = request.PageId,
            Title = request.Title,
            Model = request.Model,
            LastActivityAt = DateTime.UtcNow
        });
        return Mappers.ToDto(conversation);
    }

    public async Task<List<MessageDto>> GetMessagesAsync(Guid conversationId) =>
        (await _messages.GetByConversationIdAsync(conversationId)).Select(Mappers.ToDto).ToList();

    public async Task<MessageDto> SendMessageAsync(Guid conversationId, SendMessageRequest request)
    {
        var conversation = await _conversations.GetByIdAsync(conversationId)
            ?? throw new InvalidOperationException("Conversation not found.");

        var message = await _messages.AddAsync(new AiMessage
        {
            ConversationId = conversationId,
            Role = request.Role,
            Content = request.Content,
            ActionType = request.ActionType,
            ClientTimestamp = request.ClientTimestamp ?? DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        conversation.MessageCount += 1;
        conversation.LastActivityAt = DateTime.UtcNow;
        await _conversations.UpdateAsync(conversation);
        return Mappers.ToDto(message);
    }

    public List<AiTemplateDto> GetTemplates(string? kind = null) => StitchTemplateCatalog
        .All()
        .Where(template => string.IsNullOrWhiteSpace(kind) || template.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase))
        .Select(template => new AiTemplateDto
        {
            Key = template.Key,
            Kind = template.Kind,
            Category = template.Category,
            Label = template.Label,
            Description = template.Description,
            ThumbnailUrl = StitchTemplateCatalog.GetThumbnailUrl(template),
            PageCount = template.Pages.Count,
            PageTypes = template.Pages.Select(page => page.PageType).Distinct().ToList()
        })
        .ToList();

    public async Task<AiGenerateSiteResponse> GenerateSiteAsync(Guid userId, AiGenerateSiteRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var siteName = CleanTitle(request.SiteName, "AI Generated Site");
        var prompt = CleanPrompt(request.Prompt, siteName);

        if (!string.IsNullOrWhiteSpace(request.TemplateKey) &&
            StitchTemplateCatalog.TryGet(request.TemplateKey, out var siteTemplate) &&
            siteTemplate.Kind == "site")
        {
            return await GenerateTemplateSiteAsync(userId, request, siteName, prompt, siteTemplate, stopwatch);
        }

        var pageTypes = NormalizePageTypes(request.PageTypes);

        var site = await _sites.AddAsync(new Site
        {
            UserId = userId,
            Name = siteName,
            Description = string.IsNullOrWhiteSpace(request.Description) ? SummarizePrompt(prompt) : request.Description.Trim(),
            Slug = await CreateUniqueSiteSlugAsync(siteName)
        });

        var response = new AiGenerateSiteResponse
        {
            SiteId = site.Id,
            SiteName = site.Name,
            Slug = site.Slug ?? string.Empty
        };

        var order = 0;
        foreach (var pageType in pageTypes)
        {
            var pageName = PageNameForType(pageType);
            var generated = GeneratePageDocument(siteName, pageName, pageType, prompt, request.Style, request.ContentLength);
            var page = await _pages.AddAsync(new Page
            {
                SiteId = site.Id,
                Title = pageName,
                Slug = pageType == "home" ? "home" : await CreateUniquePageSlugAsync(site.Id, pageType),
                PageType = pageType,
                IsHome = pageType == "home",
                DisplayOrder = order++,
                ShowInNav = true,
                HtmlContent = generated.Html,
                CssContent = generated.Css,
                JsContent = generated.Js,
                Components = "[]",
                Styles = "[]",
                MetaTitle = generated.MetaTitle,
                MetaDescription = generated.MetaDescription
            });

            response.Pages.Add(ToGeneratedPageDto(page, generated, stopwatch.ElapsedMilliseconds));
        }

        response.GenerationTimeMs = stopwatch.ElapsedMilliseconds;
        response.AiSuggestions = new List<string>
        {
            "已產生可由 GrapesJS 再編輯的 HTML/CSS 頁面。",
            "可進入 Editor 後用 Blocks、Styles、Assets 繼續微調。",
            "發佈前建議替換品牌圖片與 CTA 連結。"
        };

        await RecordGenerationAsync(site.Id, response.Pages.FirstOrDefault()?.PageId, "AI generated website", prompt, response);
        return response;
    }

    public async Task<AiGeneratedPageDto> GeneratePageAsync(Guid userId, AiGeneratePageRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var site = await _sites.GetByIdAsync(request.SiteId)
            ?? throw new InvalidOperationException("Site not found.");
        if (site.UserId != userId) throw new InvalidOperationException("Site not found.");

        Page? page = null;
        if (request.PageId.HasValue)
        {
            page = await _pages.GetByIdAsync(request.PageId.Value);
            if (page is null || page.SiteId != site.Id) throw new InvalidOperationException("Page not found.");
        }

        StitchTemplateDefinition? selectedTemplate = null;
        var hasTemplate = !string.IsNullOrWhiteSpace(request.TemplateKey) &&
            StitchTemplateCatalog.TryGet(request.TemplateKey, out selectedTemplate);
        var templatePage = selectedTemplate?.Pages.FirstOrDefault();
        var pageType = hasTemplate && templatePage is not null ? templatePage.PageType : NormalizePageType(request.PageType);
        var pageName = CleanTitle(request.PageName, page?.Title ?? templatePage?.Title ?? PageNameForType(pageType));
        var prompt = CleanPrompt(request.Prompt, pageName);
        var generated = hasTemplate && selectedTemplate is not null && templatePage is not null
            ? GenerateTemplatePageDocument(site.Name, pageName, prompt, templatePage, selectedTemplate)
            : GeneratePageDocument(site.Name, pageName, pageType, prompt, request.Style, request.ContentLength);

        if (page is null)
        {
            page = await _pages.AddAsync(new Page
            {
                SiteId = site.Id,
                Title = pageName,
                Slug = await CreateUniquePageSlugAsync(site.Id, string.IsNullOrWhiteSpace(request.Slug) ? pageName : request.Slug),
                PageType = pageType,
                DisplayOrder = (await _pages.GetBySiteIdAsync(site.Id)).Count,
                ShowInNav = true,
                HtmlContent = generated.Html,
                CssContent = generated.Css,
                JsContent = generated.Js,
                Components = "[]",
                Styles = "[]",
                MetaTitle = generated.MetaTitle,
                MetaDescription = generated.MetaDescription
            });
        }
        else
        {
            page.Title = pageName;
            page.PageType = pageType;
            page.HtmlContent = generated.Html;
            page.CssContent = generated.Css;
            page.JsContent = generated.Js;
            page.Components = "[]";
            page.Styles = "[]";
            page.MetaTitle = generated.MetaTitle;
            page.MetaDescription = generated.MetaDescription;
            await _pages.UpdateAsync(page);
        }

        var response = ToGeneratedPageDto(page, generated, stopwatch.ElapsedMilliseconds);
        await RecordGenerationAsync(site.Id, page.Id, "AI generated page", prompt, response);
        return response;
    }

    private async Task<AiGenerateSiteResponse> GenerateTemplateSiteAsync(
        Guid userId,
        AiGenerateSiteRequest request,
        string siteName,
        string prompt,
        StitchTemplateDefinition siteTemplate,
        Stopwatch stopwatch)
    {
        var site = await _sites.AddAsync(new Site
        {
            UserId = userId,
            Name = siteName,
            Description = string.IsNullOrWhiteSpace(request.Description) ? siteTemplate.Description : request.Description.Trim(),
            Slug = await CreateUniqueSiteSlugAsync(siteName)
        });

        var response = new AiGenerateSiteResponse
        {
            SiteId = site.Id,
            SiteName = site.Name,
            Slug = site.Slug ?? string.Empty
        };

        var order = 0;
        foreach (var templatePage in siteTemplate.Pages)
        {
            var generated = GenerateTemplatePageDocument(site.Name, templatePage.Title, prompt, templatePage, siteTemplate);
            var page = await _pages.AddAsync(new Page
            {
                SiteId = site.Id,
                Title = templatePage.Title,
                Slug = templatePage.IsHome ? "home" : await CreateUniquePageSlugAsync(site.Id, templatePage.Slug),
                PageType = templatePage.PageType,
                IsHome = templatePage.IsHome,
                DisplayOrder = order++,
                ShowInNav = templatePage.ShowInNav,
                HtmlContent = generated.Html,
                CssContent = generated.Css,
                JsContent = generated.Js,
                Components = "[]",
                Styles = "[]",
                MetaTitle = generated.MetaTitle,
                MetaDescription = generated.MetaDescription
            });

            response.Pages.Add(ToGeneratedPageDto(page, generated, stopwatch.ElapsedMilliseconds));
        }

        response.GenerationTimeMs = stopwatch.ElapsedMilliseconds;
        response.AiSuggestions = new List<string>
        {
            $"已套用 Stitch 樣板網站「{siteTemplate.Label}」。",
            "每個頁面都保留原始 Stitch head 資源，可在發佈時維持高保真視覺。",
            "進入 Editor 後可繼續替換品牌文案、圖片、CTA 與資料欄位。"
        };

        await RecordGenerationAsync(site.Id, response.Pages.FirstOrDefault()?.PageId, "Stitch template website", prompt, response);
        return response;
    }

    private async Task RecordGenerationAsync(Guid siteId, Guid? pageId, string title, string prompt, object result)
    {
        var conversation = await _conversations.AddAsync(new AiConversation
        {
            SiteId = siteId,
            PageId = pageId,
            Title = title,
            Model = "siteforge-local-generator",
            MessageCount = 2,
            LastActivityAt = DateTime.UtcNow,
            IsCompleted = true,
            Summary = "Generated structured GrapesJS-ready website content."
        });

        await _messages.AddAsync(new AiMessage
        {
            ConversationId = conversation.Id,
            Role = "user",
            Content = prompt,
            ActionType = "generate_page",
            ClientTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        await _messages.AddAsync(new AiMessage
        {
            ConversationId = conversation.Id,
            Role = "assistant",
            Content = "Generated HTML, CSS, and editor-ready page data.",
            ActionType = "generate_page",
            ActionResult = JsonSerializer.Serialize(result),
            ClientTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
    }

    private async Task<string> CreateUniqueSiteSlugAsync(string name)
    {
        var baseSlug = Mappers.Slugify(name);
        var slug = baseSlug;
        var suffix = 1;
        while (await _sites.GetBySlugAsync(slug) is not null)
        {
            slug = $"{baseSlug}-{suffix++}";
        }
        return slug;
    }

    private async Task<string> CreateUniquePageSlugAsync(Guid siteId, string value)
    {
        var baseSlug = Mappers.Slugify(value);
        var slug = baseSlug;
        var suffix = 1;
        while (await _pages.GetBySiteAndSlugAsync(siteId, slug) is not null)
        {
            slug = $"{baseSlug}-{suffix++}";
        }
        return slug;
    }

    private static AiGeneratedPageDto ToGeneratedPageDto(Page page, GeneratedPageDocument generated, long elapsedMs) => new()
    {
        PageId = page.Id,
        SiteId = page.SiteId,
        PageName = page.Title,
        Slug = page.Slug,
        PageType = page.PageType,
        HtmlContent = generated.Html,
        CssContent = generated.Css,
        JsContent = generated.Js,
        Components = "[]",
        Styles = "[]",
        AiSuggestions = generated.Suggestions,
        GenerationTimeMs = elapsedMs
    };

    private static GeneratedPageDocument GeneratePageDocument(
        string siteName,
        string pageName,
        string pageType,
        string prompt,
        string? style,
        string? contentLength)
    {
        var palette = Palette.From(style, prompt);
        var topics = ExtractTopics(prompt, pageType);
        var escapedSite = WebUtility.HtmlEncode(siteName);
        var escapedPage = WebUtility.HtmlEncode(pageName);
        var escapedPrompt = WebUtility.HtmlEncode(prompt);
        var heroImage = ImageFor(pageType, prompt);
        var detailLevel = (contentLength ?? "medium").ToLowerInvariant();
        var paragraph = detailLevel is "long" or "extra_long"
            ? "我們把策略、內容、設計系統與可維護性放在同一個流程裡，讓頁面不只是看起來完整，也能支援後續營運、轉換追蹤與快速改版。"
            : "以清楚的訊息架構、成熟的視覺節奏與可擴充的區塊，快速產出可上線的品牌頁面。";

        var html = pageType switch
        {
            "about" => AboutHtml(escapedSite, escapedPage, escapedPrompt, topics, palette, paragraph),
            "services" => ServicesHtml(escapedSite, escapedPage, escapedPrompt, topics, palette, paragraph),
            "product" => ProductHtml(escapedSite, escapedPage, escapedPrompt, topics, palette, paragraph),
            "contact" => ContactHtml(escapedSite, escapedPage, escapedPrompt, topics, palette),
            "blog" => BlogHtml(escapedSite, escapedPage, escapedPrompt, topics, palette),
            "portfolio" => PortfolioHtml(escapedSite, escapedPage, escapedPrompt, topics, palette, paragraph),
            _ => HomeHtml(escapedSite, escapedPage, escapedPrompt, topics, palette, heroImage, paragraph)
        };

        var css = $$"""
        :root {
          --sf-ai-primary: {{palette.Primary}};
          --sf-ai-secondary: {{palette.Secondary}};
          --sf-ai-accent: {{palette.Accent}};
          --sf-ai-ink: #111827;
          --sf-ai-muted: #64748b;
          --sf-ai-line: #e5e7eb;
        }
        .sf-ai-page { color: var(--sf-ai-ink); background: #fff; }
        .sf-ai-kicker { color: var(--sf-ai-primary); font-weight: 800; letter-spacing: 0; text-transform: uppercase; font-size: .78rem; }
        .sf-ai-gradient { background: linear-gradient(135deg, {{palette.Primary}} 0%, {{palette.Secondary}} 52%, {{palette.Accent}} 100%); }
        .sf-ai-card { border: 1px solid var(--sf-ai-line); border-radius: 8px; background: rgba(255,255,255,.92); box-shadow: 0 18px 45px rgba(15,23,42,.08); }
        .sf-ai-button { display: inline-flex; align-items: center; justify-content: center; min-height: 46px; border-radius: 8px; padding: 0 20px; background: var(--sf-ai-primary); color: white; font-weight: 800; text-decoration: none; }
        .sf-ai-button.secondary { background: #fff; color: var(--sf-ai-ink); border: 1px solid var(--sf-ai-line); }
        .sf-ai-image { width: 100%; height: 100%; object-fit: cover; border-radius: 8px; }
        """;

        return new GeneratedPageDocument(
            html,
            css,
            "document.documentElement.dataset.siteforgeAi = 'generated';",
            $"{pageName} | {siteName}",
            StripTags(prompt).Length > 150 ? StripTags(prompt)[..150] : StripTags(prompt),
            new List<string>
            {
                $"已依照「{StripTags(prompt)}」生成 {pageName}。",
                "可在 GrapesJS 內選取區塊後調整 Styles / Properties。",
                "建議替換圖片、導覽連結與表單送出目標。"
            });
    }

    private static string HomeHtml(string siteName, string pageName, string prompt, List<string> topics, Palette palette, string heroImage, string paragraph) => $$"""
    <main class="sf-ai-page">
      <section class="relative overflow-hidden px-6 py-20 md:py-28" style="background: linear-gradient(135deg, #fff 0%, {{palette.Soft}} 100%);">
        <div class="mx-auto grid max-w-7xl items-center gap-12 md:grid-cols-[1.05fr_.95fr]">
          <div>
            <p class="sf-ai-kicker">AI generated website</p>
            <h1 class="mt-4 text-4xl font-black leading-tight text-gray-950 md:text-6xl">{{siteName}}：{{HeadlineFor(prompt, pageName)}}</h1>
            <p class="mt-6 max-w-2xl text-lg leading-8 text-slate-600">{{paragraph}}</p>
            <div class="mt-8 flex flex-wrap gap-3">
              <a class="sf-ai-button" href="#contact">開始合作</a>
              <a class="sf-ai-button secondary" href="#features">查看亮點</a>
            </div>
          </div>
          <div class="sf-ai-card p-3">
            <img class="sf-ai-image aspect-[4/3]" src="{{heroImage}}" alt="{{siteName}} visual preview" />
          </div>
        </div>
      </section>
      <section id="features" class="px-6 py-16">
        <div class="mx-auto max-w-7xl">
          <p class="sf-ai-kicker">Core sections</p>
          <h2 class="mt-3 text-3xl font-black md:text-4xl">為 {{siteName}} 建立清楚的第一印象</h2>
          <div class="mt-9 grid gap-5 md:grid-cols-3">
            {{CardsHtml(topics.Take(3).ToList(), palette)}}
          </div>
        </div>
      </section>
      <section class="px-6 py-16" style="background:#f8fafc;">
        <div class="mx-auto grid max-w-7xl gap-8 md:grid-cols-3">
          <div class="md:col-span-1">
            <p class="sf-ai-kicker">Request</p>
            <h2 class="mt-3 text-3xl font-black">生成依據</h2>
          </div>
          <p class="text-lg leading-8 text-slate-600 md:col-span-2">{{prompt}}</p>
        </div>
      </section>
      <section id="contact" class="px-6 py-16">
        <div class="sf-ai-card mx-auto max-w-5xl p-8 md:p-10">
          <p class="sf-ai-kicker">Next step</p>
          <h2 class="mt-3 text-3xl font-black">把這個初稿帶進編輯器完成品牌化</h2>
          <p class="mt-4 text-slate-600">你可以接著調整圖片、顏色、區塊順序與文案，然後直接發佈成靜態網站。</p>
        </div>
      </section>
    </main>
    """;

    private static string AboutHtml(string siteName, string pageName, string prompt, List<string> topics, Palette palette, string paragraph) => $$"""
    <main class="sf-ai-page">
      <section class="px-6 py-20" style="background:{{palette.Soft}};">
        <div class="mx-auto max-w-5xl text-center">
          <p class="sf-ai-kicker">About</p>
          <h1 class="mt-4 text-4xl font-black md:text-6xl">{{pageName}}</h1>
          <p class="mx-auto mt-6 max-w-3xl text-lg leading-8 text-slate-600">{{paragraph}}</p>
        </div>
      </section>
      <section class="px-6 py-16">
        <div class="mx-auto grid max-w-7xl gap-6 md:grid-cols-3">{{CardsHtml(topics.Take(3).ToList(), palette)}}</div>
      </section>
      <section class="px-6 py-16">
        <div class="mx-auto max-w-4xl border-l-4 pl-8" style="border-color:{{palette.Primary}};">
          <p class="sf-ai-kicker">Story</p>
          <h2 class="mt-3 text-3xl font-black">{{siteName}} 的定位</h2>
          <p class="mt-5 text-lg leading-8 text-slate-600">{{prompt}}</p>
        </div>
      </section>
    </main>
    """;

    private static string ServicesHtml(string siteName, string pageName, string prompt, List<string> topics, Palette palette, string paragraph) => $$"""
    <main class="sf-ai-page">
      <section class="px-6 py-20">
        <div class="mx-auto max-w-7xl">
          <p class="sf-ai-kicker">Services</p>
          <h1 class="mt-4 max-w-3xl text-4xl font-black md:text-6xl">{{pageName}}</h1>
          <p class="mt-6 max-w-3xl text-lg leading-8 text-slate-600">{{paragraph}}</p>
          <div class="mt-10 grid gap-5 md:grid-cols-3">{{CardsHtml(topics.Take(6).ToList(), palette)}}</div>
        </div>
      </section>
    </main>
    """;

    private static string ProductHtml(string siteName, string pageName, string prompt, List<string> topics, Palette palette, string paragraph) => $$"""
    <main class="sf-ai-page">
      <section class="px-6 py-20" style="background:#f8fafc;">
        <div class="mx-auto max-w-7xl">
          <p class="sf-ai-kicker">Product</p>
          <h1 class="mt-4 text-4xl font-black md:text-6xl">{{siteName}} 產品方案</h1>
          <p class="mt-6 max-w-3xl text-lg leading-8 text-slate-600">{{paragraph}}</p>
          <div class="mt-10 grid gap-5 md:grid-cols-3">{{CardsHtml(topics.Take(3).ToList(), palette)}}</div>
        </div>
      </section>
    </main>
    """;

    private static string ContactHtml(string siteName, string pageName, string prompt, List<string> topics, Palette palette) => $$"""
    <main class="sf-ai-page">
      <section class="px-6 py-20">
        <div class="mx-auto grid max-w-7xl gap-10 md:grid-cols-2">
          <div>
            <p class="sf-ai-kicker">Contact</p>
            <h1 class="mt-4 text-4xl font-black md:text-6xl">聯絡 {{siteName}}</h1>
            <p class="mt-6 text-lg leading-8 text-slate-600">{{prompt}}</p>
          </div>
          <form class="sf-ai-card grid gap-4 p-6">
            <input class="rounded-md border border-slate-200 px-4 py-3" placeholder="Name" />
            <input class="rounded-md border border-slate-200 px-4 py-3" placeholder="Email" />
            <textarea class="min-h-32 rounded-md border border-slate-200 px-4 py-3" placeholder="Message"></textarea>
            <button class="sf-ai-button" type="button">送出需求</button>
          </form>
        </div>
      </section>
    </main>
    """;

    private static string BlogHtml(string siteName, string pageName, string prompt, List<string> topics, Palette palette) => $$"""
    <main class="sf-ai-page">
      <section class="px-6 py-20" style="background:{{palette.Soft}};">
        <div class="mx-auto max-w-7xl">
          <p class="sf-ai-kicker">Editorial</p>
          <h1 class="mt-4 text-4xl font-black md:text-6xl">{{pageName}}</h1>
          <p class="mt-6 max-w-3xl text-lg leading-8 text-slate-600">{{prompt}}</p>
          <div class="mt-10 grid gap-5 md:grid-cols-3">{{CardsHtml(topics.Take(3).ToList(), palette)}}</div>
        </div>
      </section>
    </main>
    """;

    private static string PortfolioHtml(string siteName, string pageName, string prompt, List<string> topics, Palette palette, string paragraph) => $$"""
    <main class="sf-ai-page">
      <section class="px-6 py-20">
        <div class="mx-auto max-w-7xl">
          <p class="sf-ai-kicker">Portfolio</p>
          <h1 class="mt-4 text-4xl font-black md:text-6xl">{{pageName}}</h1>
          <p class="mt-6 max-w-3xl text-lg leading-8 text-slate-600">{{paragraph}}</p>
          <div class="mt-10 grid gap-5 md:grid-cols-3">{{CardsHtml(topics.Take(6).ToList(), palette)}}</div>
        </div>
      </section>
    </main>
    """;

    private static string CardsHtml(List<string> topics, Palette palette)
    {
        if (topics.Count == 0)
        {
            topics = new List<string> { "清楚定位", "快速改版", "完整發佈" };
        }

        return string.Join("\n", topics.Select((topic, index) => $$"""
        <article class="sf-ai-card p-6">
          <div class="mb-5 flex h-11 w-11 items-center justify-center rounded-md text-white" style="background:{{(index % 2 == 0 ? palette.Primary : palette.Secondary)}};">{{index + 1}}</div>
          <h3 class="text-xl font-black">{{WebUtility.HtmlEncode(topic)}}</h3>
          <p class="mt-3 leading-7 text-slate-600">以可編輯區塊呈現重點，讓內容、樣式與後續維護都能在 SiteForge 裡完成。</p>
        </article>
        """));
    }

    private static List<string> ExtractTopics(string prompt, string pageType)
    {
        var separators = new[] { '\n', '、', ',', '，', ';', '；', '.', '。' };
        var topics = prompt
            .Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(StripTags)
            .Select(x => x.Trim())
            .Where(x => x.Length is >= 2 and <= 36)
            .Take(6)
            .ToList();

        if (topics.Count >= 3) return topics;

        var defaults = pageType switch
        {
            "about" => new[] { "品牌故事", "使命願景", "團隊優勢" },
            "services" => new[] { "策略規劃", "設計執行", "成效優化" },
            "product" => new[] { "核心功能", "使用情境", "方案優勢" },
            "contact" => new[] { "需求溝通", "合作流程", "快速回覆" },
            "portfolio" => new[] { "精選案例", "設計成果", "客戶價值" },
            _ => new[] { "品牌定位", "服務亮點", "行動轉換" }
        };
        topics.AddRange(defaults.Where(item => !topics.Contains(item)));
        return topics.Take(6).ToList();
    }

    private static List<string> NormalizePageTypes(List<string>? pageTypes)
    {
        var normalized = (pageTypes is { Count: > 0 } ? pageTypes : new List<string> { "home", "about", "services", "contact" })
            .Select(NormalizePageType)
            .Distinct()
            .ToList();

        if (!normalized.Contains("home")) normalized.Insert(0, "home");
        return normalized.Take(6).ToList();
    }

    private static string NormalizePageType(string? pageType)
    {
        var type = (pageType ?? "custom").Trim().ToLowerInvariant();
        return type switch
        {
            "homepage" or "index" => "home",
            "service" => "services",
            "products" => "product",
            "work" or "case" => "portfolio",
            "" => "custom",
            _ => type
        };
    }

    private static string PageNameForType(string pageType) => pageType switch
    {
        "home" => "Home",
        "about" => "About",
        "services" => "Services",
        "product" => "Products",
        "blog" => "Blog",
        "contact" => "Contact",
        "portfolio" => "Portfolio",
        _ => "Custom Page"
    };

    private static string CleanTitle(string value, string fallback)
    {
        var title = StripTags(value).Trim();
        return string.IsNullOrWhiteSpace(title) ? fallback : title.Length > 90 ? title[..90] : title;
    }

    private static string CleanPrompt(string value, string fallback)
    {
        var prompt = StripTags(value).Trim();
        return string.IsNullOrWhiteSpace(prompt)
            ? $"Create a modern, conversion-focused page for {fallback}."
            : prompt.Length > 1800 ? prompt[..1800] : prompt;
    }

    private static string SummarizePrompt(string prompt) => prompt.Length > 160 ? prompt[..160] : prompt;

    private static string StripTags(string value) => string.IsNullOrWhiteSpace(value)
        ? string.Empty
        : WebUtility.HtmlDecode(value.Replace("<", string.Empty).Replace(">", string.Empty));

    private static string HeadlineFor(string prompt, string pageName)
    {
        var clean = StripTags(prompt);
        if (clean.Length < 16) return pageName;
        return clean.Length > 46 ? clean[..46] : clean;
    }

    private static string ImageFor(string pageType, string prompt)
    {
        var query = pageType switch
        {
            "product" => "productivity dashboard",
            "services" => "creative studio team",
            "about" => "team collaboration office",
            "portfolio" => "design workspace",
            _ => "modern business workspace"
        };
        return $"https://source.unsplash.com/1200x900/?{Uri.EscapeDataString(query)}";
    }

    private static GeneratedPageDocument GenerateTemplatePageDocument(
        string siteName,
        string pageName,
        string prompt,
        StitchTemplatePage templatePage,
        StitchTemplateDefinition template)
    {
        var source = StitchTemplateCatalog.Load(templatePage.FileName);
        var escapedTemplateKey = WebUtility.HtmlEncode(template.Key);
        var html = ReplaceBrandTokens(source.Body, template.DefaultBrandName, siteName);
        var head = ReplaceBrandTokens(source.Head, template.DefaultBrandName, siteName);
        var bodyClass = string.IsNullOrWhiteSpace(source.BodyClass) ? string.Empty : $" {source.BodyClass}";

        html = $$"""
        <div class="siteforge-stitch-template{{bodyClass}}" data-siteforge-template="{{escapedTemplateKey}}" data-siteforge-template-page="{{WebUtility.HtmlEncode(templatePage.PageType)}}">
          {{html}}
        </div>
        """;

        var js = $$"""
        /*SITEFORGE_TEMPLATE_HEAD_START
        {{head}}
        SITEFORGE_TEMPLATE_HEAD_END*/
        document.documentElement.dataset.siteforgeTemplate = "{{template.Key}}";
        document.documentElement.dataset.siteforgeTemplatePage = "{{templatePage.PageType}}";
        """;

        var css = """
        .siteforge-stitch-template { min-height: 100vh; }
        .siteforge-stitch-template img { max-width: 100%; }
        """;

        return new GeneratedPageDocument(
            html,
            css,
            js,
            $"{pageName} | {siteName}",
            string.IsNullOrWhiteSpace(prompt) ? template.Description : SummarizePrompt(prompt),
            new List<string>
            {
                $"已套用 Stitch 樣板「{template.Label} / {templatePage.Title}」。",
                "此頁保留 Stitch 匯出的 Tailwind、字體與 Material Symbols 設定。",
                "建議替換圖片、品牌名稱、CTA 連結與資料欄位後再發佈。"
            });
    }

    private static string ReplaceBrandTokens(string value, string defaultBrandName, string siteName)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var output = value
            .Replace(defaultBrandName, WebUtility.HtmlEncode(siteName), StringComparison.OrdinalIgnoreCase)
            .Replace("MUJI-INSPIRED RETAIL", WebUtility.HtmlEncode(siteName), StringComparison.OrdinalIgnoreCase)
            .Replace("Muji-Inspired Retail", WebUtility.HtmlEncode(siteName), StringComparison.OrdinalIgnoreCase)
            .Replace("AETHERIS 瑰麗美學", WebUtility.HtmlEncode(siteName), StringComparison.OrdinalIgnoreCase)
            .Replace("VINTAGE &amp; VINE DISTRIBUTORS", WebUtility.HtmlEncode(siteName), StringComparison.OrdinalIgnoreCase)
            .Replace("VINTAGE & VINE DISTRIBUTORS", WebUtility.HtmlEncode(siteName), StringComparison.OrdinalIgnoreCase)
            .Replace("VINTAGE &amp; VINE", WebUtility.HtmlEncode(siteName), StringComparison.OrdinalIgnoreCase)
            .Replace("VINTAGE & VINE", WebUtility.HtmlEncode(siteName), StringComparison.OrdinalIgnoreCase)
            .Replace("DPP Explorer", WebUtility.HtmlEncode(siteName), StringComparison.OrdinalIgnoreCase)
            .Replace("VeriShield AI", WebUtility.HtmlEncode(siteName), StringComparison.OrdinalIgnoreCase);

        return output;
    }

    private sealed record GeneratedPageDocument(
        string Html,
        string Css,
        string Js,
        string MetaTitle,
        string MetaDescription,
        List<string> Suggestions);

    private sealed record StitchTemplateDefinition(
        string Key,
        string Kind,
        string Category,
        string Label,
        string Description,
        string DefaultBrandName,
        List<StitchTemplatePage> Pages);

    private sealed record StitchTemplatePage(
        string Title,
        string Slug,
        string PageType,
        string FileName,
        bool IsHome = false,
        bool ShowInNav = true);

    private sealed record StitchTemplateSource(string Head, string Body, string BodyClass);

    private static class StitchTemplateCatalog
    {
        private static readonly List<StitchTemplateDefinition> Templates = new()
        {
            new("site-retail", "site", "零售業", "零售業品牌網站", "從 Stitch 匯入的零售品牌完整網站，包含首頁、關於、產品、會員服務、門市、消息與聯絡頁。", "MUJI-INSPIRED RETAIL", new()
            {
                new("Home", "home", "home", "site_retail_home.html", true),
                new("About", "about", "about", "site_retail_about.html"),
                new("Products", "products", "product", "site_retail_product.html"),
                new("Services", "services", "services", "site_retail_services.html"),
                new("Locations", "locations", "locations", "site_retail_locations.html"),
                new("News", "news", "blog", "site_retail_news.html"),
                new("Contact", "contact", "contact", "site_retail_contact.html")
            }),
            new("site-beauty", "site", "美妝業", "美妝保養品牌網站", "從 Stitch 匯入的精品美妝網站，包含品牌首頁、產品、成分、顧問服務與聯絡頁。", "AETHERIS 瑰麗美學", new()
            {
                new("Home", "home", "home", "site_beauty_home.html", true),
                new("Products", "products", "product", "site_beauty_product.html"),
                new("Ingredients", "ingredients", "ingredients", "site_beauty_ingredients.html"),
                new("Consultation", "consultation", "services", "site_beauty_services.html"),
                new("Contact", "contact", "contact", "site_beauty_contact.html")
            }),
            new("site-beverage", "site", "酒水業", "酒水品牌與通路網站", "從 Stitch 匯入的高質感酒水品牌網站，包含首頁、品牌故事、產品系列、通路合作、部落格與聯絡頁。", "VINTAGE & VINE", new()
            {
                new("Home", "home", "home", "site_beverage_home.html", true),
                new("About", "about", "about", "site_beverage_about.html"),
                new("Collections", "collections", "product", "site_beverage_product.html"),
                new("Partnership", "partnership", "services", "site_beverage_services.html"),
                new("Journal", "journal", "blog", "site_beverage_blog.html"),
                new("Contact", "contact", "contact", "site_beverage_contact.html")
            }),
            new("site-3c", "site", "3C 產業", "3C 科技平台網站", "從 Stitch 匯入的 3C 與硬體解決方案網站，包含首頁、關於、產品、服務、支援與聯絡頁。", "ApexEdge", new()
            {
                new("Home", "home", "home", "site_3c_home.html", true),
                new("About", "about", "about", "site_3c_about.html"),
                new("Products", "products", "product", "site_3c_product.html"),
                new("Solutions", "solutions", "services", "site_3c_services.html"),
                new("Support", "support", "support", "site_3c_support.html"),
                new("Contact", "contact", "contact", "site_3c_contact.html")
            }),
            new("page-anti-counterfeit", "page", "樣板網頁", "防偽顯示網頁", "掃碼或輸入序號後顯示正品驗證結果、產品資訊與客服回報 CTA。", "VeriShield AI", new()
            {
                new("防偽驗證", "anti-counterfeit", "anti-counterfeit", "page_anti_counterfeit.html", true)
            }),
            new("page-scan-result", "page", "樣板網頁", "掃碼顯示網頁", "通用掃碼結果頁，適合商品資訊、活動入口、文件下載與會員互動。", "ScanLink", new()
            {
                new("掃碼結果", "scan-result", "scan-result", "page_scan_result.html", true)
            }),
            new("page-lottery", "page", "樣板網頁", "抽獎顯示網頁", "掃碼抽獎結果頁，包含中獎狀態、獎項、兌換碼與活動規則。", "Grand Sweepstakes", new()
            {
                new("抽獎結果", "lottery", "lottery", "page_lottery_result.html", true)
            }),
            new("page-points-redemption", "page", "樣板網頁", "點數兌換顯示網頁", "會員點數兌換結果頁，包含點數餘額、兌換品項、條碼與推薦兌換。", "PointsPlus", new()
            {
                new("點數兌換", "points-redemption", "points-redemption", "page_points_redemption.html", true)
            }),
            new("page-traceability", "page", "樣板網頁", "追蹤追溯顯示網頁", "產品追蹤追溯頁，呈現批號、來源、檢驗、物流與通路時間軸。", "TraceFlow", new()
            {
                new("追蹤追溯", "traceability", "traceability", "page_traceability.html", true)
            }),
            new("page-dpp", "page", "樣板網頁", "DPP 顯示網頁", "Digital Product Passport 顯示頁，呈現產品身份、材料、合規、永續、維修與回收資訊。", "DPP Explorer", new()
            {
                new("DPP 數位產品護照", "dpp", "dpp", "page_dpp.html", true)
            })
        };

        public static List<StitchTemplateDefinition> All() => Templates;

        public static string GetThumbnailUrl(StitchTemplateDefinition template)
        {
            var pages = template.Pages
                .OrderByDescending(page => page.IsHome)
                .ThenBy(page => page.ShowInNav ? 0 : 1);

            foreach (var page in pages)
            {
                try
                {
                    var source = Load(page.FileName);
                    var url = ExtractFirstImageUrl($"{source.Head}\n{source.Body}");
                    if (!string.IsNullOrWhiteSpace(url)) return url;
                }
                catch
                {
                    // Thumbnail discovery should not make the template catalog fail.
                }
            }

            return string.Empty;
        }

        public static bool TryGet(string? key, out StitchTemplateDefinition template)
        {
            template = Templates.FirstOrDefault(item => item.Key.Equals(key?.Trim(), StringComparison.OrdinalIgnoreCase))!;
            return template is not null;
        }

        public static StitchTemplateSource Load(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Stitch", fileName);
            if (!File.Exists(path))
            {
                throw new InvalidOperationException($"Template file not found: {fileName}");
            }

            var html = File.ReadAllText(path);
            var head = NormalizeHead(MatchInner(html, "head"));
            var body = MatchInner(html, "body");
            var bodyClass = Regex.Match(html, "<body[^>]*class=\"(?<class>[^\"]*)\"", RegexOptions.IgnoreCase).Groups["class"].Value;
            return new StitchTemplateSource(head, string.IsNullOrWhiteSpace(body) ? html : body, bodyClass);
        }

        private static string NormalizeHead(string head)
        {
            if (string.IsNullOrWhiteSpace(head)) return string.Empty;

            var configMatch = Regex.Match(
                head,
                @"<script\s+id=""tailwind-config""[^>]*>.*?</script>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var cdnMatch = Regex.Match(
                head,
                @"<script[^>]+src=""https://cdn\.tailwindcss\.com[^""]*""[^>]*>\s*</script>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (!configMatch.Success || !cdnMatch.Success || configMatch.Index < cdnMatch.Index)
            {
                return head;
            }

            var withoutConfig = head.Remove(configMatch.Index, configMatch.Length).Trim();
            var nextCdnMatch = Regex.Match(
                withoutConfig,
                @"<script[^>]+src=""https://cdn\.tailwindcss\.com[^""]*""[^>]*>\s*</script>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            return nextCdnMatch.Success
                ? withoutConfig.Insert(nextCdnMatch.Index, $"{configMatch.Value}\n")
                : $"{configMatch.Value}\n{withoutConfig}";
        }

        private static string MatchInner(string html, string tag)
        {
            var match = Regex.Match(html, $@"<{tag}[^>]*>(?<content>.*?)</{tag}>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            return match.Success ? match.Groups["content"].Value.Trim() : string.Empty;
        }

        private static string ExtractFirstImageUrl(string html)
        {
            var imageMatches = Regex.Matches(html, "<img[^>]+src=[\"'](?<url>https?://[^\"']+)[\"']", RegexOptions.IgnoreCase);
            var backgroundMatches = Regex.Matches(html, "url\\(['\"]?(?<url>https?://[^)'\"\\s]+)['\"]?\\)", RegexOptions.IgnoreCase);

            return imageMatches
                .Cast<Match>()
                .Concat(backgroundMatches.Cast<Match>())
                .Where(match => match.Success)
                .OrderBy(match => match.Index)
                .Select(match => match.Groups["url"].Value.Trim())
                .FirstOrDefault(url => !string.IsNullOrWhiteSpace(url)) ?? string.Empty;
        }
    }

    private sealed record Palette(string Primary, string Secondary, string Accent, string Soft)
    {
        public static Palette From(string? style, string prompt)
        {
            var value = $"{style} {prompt}".ToLowerInvariant();
            if (value.Contains("luxury") || value.Contains("premium") || value.Contains("高級"))
                return new Palette("#111827", "#9f7aea", "#f6c177", "#f8fafc");
            if (value.Contains("eco") || value.Contains("green") || value.Contains("自然"))
                return new Palette("#0f766e", "#16a34a", "#f59e0b", "#ecfdf5");
            if (value.Contains("pink") || value.Contains("fashion") || value.Contains("美"))
                return new Palette("#e11d48", "#7c3aed", "#fbbf24", "#fff1f2");
            if (value.Contains("tech") || value.Contains("ai") || value.Contains("科技"))
                return new Palette("#2563eb", "#7c3aed", "#06b6d4", "#eff6ff");
            return new Palette("#8358ed", "#fc549e", "#ffcb47", "#faf5ff");
        }
    }
}
