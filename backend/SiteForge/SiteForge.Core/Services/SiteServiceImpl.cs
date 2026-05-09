using SiteForge.Core.DTOs;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;
using SiteForge.Core.Interfaces.Services;
using SiteForge.Core.Utilities;
using System.Net;
using System.Text;

namespace SiteForge.Core.Services;

public class SiteServiceImpl : SiteService
{
    private readonly RSiteRepository _sites;
    private readonly RPageRepository _pages;
    private readonly RSiteDomainRepository _domains;
    private readonly RPublishTaskRepository _publishTasks;

    public SiteServiceImpl(RSiteRepository sites, RPageRepository pages, RSiteDomainRepository domains, RPublishTaskRepository publishTasks)
    {
        _sites = sites;
        _pages = pages;
        _domains = domains;
        _publishTasks = publishTasks;
    }

    public async Task<List<SiteDto>> GetByUserAsync(Guid userId) =>
        (await _sites.GetByUserIdAsync(userId)).Select(Mappers.ToDto).ToList();

    public async Task<SiteDto?> GetByIdAsync(Guid userId, Guid id)
    {
        var site = await _sites.GetByIdAsync(id);
        if (site is null || site.UserId != userId) return null;

        var dto = Mappers.ToDto(site);
        dto.Pages = (await _pages.GetBySiteIdAsync(site.Id)).Select(Mappers.ToLinkDto).ToList();
        dto.Domains = (await _domains.GetBySiteIdAsync(site.Id)).Select(Mappers.ToDto).ToList();
        return dto;
    }

    public async Task<SiteDto> CreateAsync(Guid userId, CreateSiteRequest request)
    {
        var site = await _sites.AddAsync(new Site
        {
            UserId = userId,
            Name = request.Name.Trim(),
            Description = request.Description,
            Slug = await CreateUniqueSlugAsync(request.Name)
        });

        await _pages.AddAsync(new Page
        {
            SiteId = site.Id,
            Title = "Home",
            Slug = "home",
            PageType = "home",
            IsHome = true,
            DisplayOrder = 0
        });

        return Mappers.ToDto(site);
    }

    public async Task<SiteDto?> UpdateAsync(Guid userId, Guid id, UpdateSiteRequest request)
    {
        var site = await _sites.GetByIdAsync(id);
        if (site is null || site.UserId != userId) return null;

        if (request.Name is not null) site.Name = request.Name.Trim();
        if (request.Description is not null) site.Description = request.Description;
        if (request.LogoUrl is not null) site.LogoUrl = request.LogoUrl;
        if (request.FaviconUrl is not null) site.FaviconUrl = request.FaviconUrl;
        if (request.CustomHeaderScript is not null) site.CustomHeaderScript = request.CustomHeaderScript;
        if (request.CustomFooterScript is not null) site.CustomFooterScript = request.CustomFooterScript;

        await _sites.UpdateAsync(site);
        return Mappers.ToDto(site);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid id)
    {
        var site = await _sites.GetByIdAsync(id);
        return site is not null && site.UserId == userId && await _sites.DeleteAsync(id);
    }

    public async Task<PublishTaskDto?> PublishAsync(Guid userId, Guid siteId, PublishRequest request)
    {
        var site = await _sites.GetByIdAsync(siteId);
        if (site is null || site.UserId != userId) return null;

        var pages = await _pages.GetBySiteIdAsync(siteId);
        var task = await _publishTasks.AddAsync(new PublishTask
        {
            SiteId = siteId,
            TaskType = request.TaskType,
            Status = "publishing",
            TotalPages = pages.Count,
            StartedAt = DateTime.UtcNow,
            TargetUrl = request.TargetUrl
        });

        try
        {
            var publishFolder = Mappers.Slugify(site.Slug ?? site.Name);
            var publishRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "published", publishFolder);
            Directory.CreateDirectory(publishRoot);

            foreach (var existingFile in Directory.EnumerateFiles(publishRoot, "*.html", SearchOption.AllDirectories))
            {
                File.Delete(existingFile);
            }

            foreach (var page in pages)
            {
                var pageFolder = page.IsHome ? publishRoot : Path.Combine(publishRoot, Mappers.Slugify(page.Slug));
                Directory.CreateDirectory(pageFolder);
                await File.WriteAllTextAsync(Path.Combine(pageFolder, "index.html"), RenderPage(site, page, pages), Encoding.UTF8);

                page.IsPublished = true;
                page.PublishedAt = DateTime.UtcNow;
                await _pages.UpdateAsync(page);
            }

            site.Status = "published";
            site.PublishedAt = DateTime.UtcNow;
            site.PublishedUrl = $"/published/{publishFolder}/";
            await _sites.UpdateAsync(site);

            task.Status = "done";
            task.PublishedPages = pages.Count;
            task.TargetUrl = string.IsNullOrWhiteSpace(request.TargetUrl) ? site.PublishedUrl : request.TargetUrl;
            task.CompletedAt = DateTime.UtcNow;
            task.Log = $"Published {pages.Count} page(s) to wwwroot/published/{publishFolder}.";
        }
        catch (Exception ex)
        {
            task.Status = "failed";
            task.ErrorMessage = ex.Message;
            task.CompletedAt = DateTime.UtcNow;
        }

        await _publishTasks.UpdateAsync(task);
        return Mappers.ToDto(task);
    }

    private async Task<string> CreateUniqueSlugAsync(string name)
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

    private static string RenderPage(Site site, Page page, List<Page> pages)
    {
        var title = WebUtility.HtmlEncode(page.MetaTitle ?? page.Title);
        var description = WebUtility.HtmlEncode(page.MetaDescription ?? site.Description ?? string.Empty);
        var body = string.IsNullOrWhiteSpace(page.HtmlContent) ? RenderEmptyPage(page) : page.HtmlContent;
        var css = page.CssContent ?? string.Empty;
        var (templateHead, js) = SplitTemplateHead(page.JsContent ?? string.Empty);
        var headerScript = site.CustomHeaderScript ?? string.Empty;
        var footerScript = site.CustomFooterScript ?? string.Empty;
        var isStitchTemplate = !string.IsNullOrWhiteSpace(templateHead) || body.Contains("siteforge-stitch-template", StringComparison.OrdinalIgnoreCase);
        var nav = isStitchTemplate ? string.Empty : RenderNavigation(site, pages);
        var tailwindScript = templateHead.Contains("cdn.tailwindcss.com", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : """<script src="https://cdn.tailwindcss.com"></script>""";

        return $$"""
        <!doctype html>
        <html lang="zh-Hant">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1">
          <title>{{title}}</title>
          <meta name="description" content="{{description}}">
          {{templateHead}}
          {{tailwindScript}}
          {{headerScript}}
          <style>
            body { margin: 0; font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif; }
            a { color: inherit; }
            {{css}}
          </style>
        </head>
        <body>
          {{nav}}
          {{body}}
          {{footerScript}}
          <script>{{js}}</script>
        </body>
        </html>
        """;
    }

    private static (string Head, string Js) SplitTemplateHead(string js)
    {
        const string start = "/*SITEFORGE_TEMPLATE_HEAD_START";
        const string end = "SITEFORGE_TEMPLATE_HEAD_END*/";
        var startIndex = js.IndexOf(start, StringComparison.Ordinal);
        if (startIndex < 0) return (string.Empty, js);

        var contentStart = startIndex + start.Length;
        var endIndex = js.IndexOf(end, contentStart, StringComparison.Ordinal);
        if (endIndex < 0) return (string.Empty, js);

        var head = js[contentStart..endIndex].Trim();
        var remaining = (js[..startIndex] + js[(endIndex + end.Length)..]).Trim();
        return (head, remaining);
    }

    private static string RenderNavigation(Site site, List<Page> pages)
    {
        var visiblePages = pages
            .Where(page => page.ShowInNav)
            .OrderBy(page => page.DisplayOrder)
            .ToList();

        if (visiblePages.Count == 0) return string.Empty;

        var links = visiblePages.Select(page =>
        {
            var href = page.IsHome ? "./" : $"./{Mappers.Slugify(page.Slug)}/";
            return $"""<a href="{href}">{WebUtility.HtmlEncode(page.Title)}</a>""";
        });

        return $"""
        <header style="position:sticky;top:0;z-index:20;background:rgba(255,255,255,.92);border-bottom:1px solid #e5e7eb;backdrop-filter:blur(12px);">
          <nav style="max-width:1120px;margin:0 auto;padding:14px 20px;display:flex;align-items:center;justify-content:space-between;gap:20px;">
            <a href="./" style="font-weight:800;text-decoration:none;color:#111827;">{WebUtility.HtmlEncode(site.Name)}</a>
            <div style="display:flex;gap:18px;flex-wrap:wrap;color:#334155;font-size:14px;">{string.Join("", links)}</div>
          </nav>
        </header>
        """;
    }

    private static string RenderEmptyPage(Page page) =>
        $"""
        <main style="padding:72px 24px;max-width:960px;margin:0 auto;">
          <p style="font-size:14px;font-weight:700;color:#2563eb;margin-bottom:12px;">SiteForge</p>
          <h1 style="font-size:42px;line-height:1.1;margin:0 0 16px;color:#111827;">{WebUtility.HtmlEncode(page.Title)}</h1>
          <p style="font-size:18px;line-height:1.7;color:#64748b;">This page has not been designed yet.</p>
        </main>
        """;
}
