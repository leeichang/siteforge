using SiteForge.Core.DTOs;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;
using SiteForge.Core.Interfaces.Services;
using SiteForge.Core.Utilities;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

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

    public async Task<AiGenerateSiteResponse> GenerateSiteAsync(Guid userId, AiGenerateSiteRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        var siteName = CleanTitle(request.SiteName, "AI Generated Site");
        var prompt = CleanPrompt(request.Prompt, siteName);
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

        var pageName = CleanTitle(request.PageName, page?.Title ?? PageNameForType(request.PageType));
        var pageType = NormalizePageType(request.PageType);
        var prompt = CleanPrompt(request.Prompt, pageName);
        var generated = GeneratePageDocument(site.Name, pageName, pageType, prompt, request.Style, request.ContentLength);

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

    private sealed record GeneratedPageDocument(
        string Html,
        string Css,
        string Js,
        string MetaTitle,
        string MetaDescription,
        List<string> Suggestions);

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
