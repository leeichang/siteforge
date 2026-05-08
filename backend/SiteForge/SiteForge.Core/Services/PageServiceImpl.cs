using SiteForge.Core.DTOs;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;
using SiteForge.Core.Interfaces.Services;
using SiteForge.Core.Utilities;

namespace SiteForge.Core.Services;

public class PageServiceImpl : PageService
{
    private readonly RPageRepository _pages;
    private readonly RWidgetBaseRepository _widgets;
    private readonly RSiteRepository _sites;

    public PageServiceImpl(RPageRepository pages, RWidgetBaseRepository widgets, RSiteRepository sites)
    {
        _pages = pages;
        _widgets = widgets;
        _sites = sites;
    }

    public async Task<List<PageDto>> GetBySiteAsync(Guid userId, Guid siteId)
    {
        return await UserOwnsSiteAsync(userId, siteId)
            ? (await _pages.GetBySiteIdAsync(siteId)).Select(Mappers.ToDto).ToList()
            : new List<PageDto>();
    }

    public async Task<PageDetailDto?> GetByIdAsync(Guid userId, Guid id)
    {
        var page = await _pages.GetByIdAsync(id);
        if (page is null || !await UserOwnsSiteAsync(userId, page.SiteId)) return null;

        var dto = Mappers.ToDetailDto(page);
        dto.Widgets = (await _widgets.GetByPageIdAsync(id)).Select(Mappers.ToDto).ToList();
        return dto;
    }

    public async Task<PageDto?> CreateAsync(Guid userId, Guid siteId, CreatePageRequest request)
    {
        if (!await UserOwnsSiteAsync(userId, siteId)) return null;

        var page = await _pages.AddAsync(new Page
        {
            SiteId = siteId,
            Title = request.Title.Trim(),
            Slug = string.IsNullOrWhiteSpace(request.Slug) ? Mappers.Slugify(request.Title) : Mappers.Slugify(request.Slug),
            ParentId = request.ParentId,
            LayoutId = request.LayoutId,
            PageType = request.PageType,
            IsHome = request.IsHome
        });
        return Mappers.ToDto(page);
    }

    public async Task<PageDto?> UpdateAsync(Guid userId, Guid id, UpdatePageRequest request)
    {
        var page = await _pages.GetByIdAsync(id);
        if (page is null || !await UserOwnsSiteAsync(userId, page.SiteId)) return null;

        if (request.Title is not null) page.Title = request.Title.Trim();
        if (request.Slug is not null) page.Slug = Mappers.Slugify(request.Slug);
        if (request.ParentId.HasValue) page.ParentId = request.ParentId;
        if (request.LayoutId.HasValue) page.LayoutId = request.LayoutId;
        if (request.PageType is not null) page.PageType = request.PageType;
        if (request.IsHome.HasValue) page.IsHome = request.IsHome.Value;
        if (request.Components is not null) page.Components = request.Components;
        if (request.Styles is not null) page.Styles = request.Styles;
        if (request.HtmlContent is not null) page.HtmlContent = request.HtmlContent;
        if (request.CssContent is not null) page.CssContent = request.CssContent;
        if (request.JsContent is not null) page.JsContent = request.JsContent;
        if (request.MetaTitle is not null) page.MetaTitle = request.MetaTitle;
        if (request.MetaDescription is not null) page.MetaDescription = request.MetaDescription;
        if (request.MetaKeywords is not null) page.MetaKeywords = request.MetaKeywords;
        if (request.DisplayOrder.HasValue) page.DisplayOrder = request.DisplayOrder.Value;
        if (request.ShowInNav.HasValue) page.ShowInNav = request.ShowInNav.Value;

        await _pages.UpdateAsync(page);
        return Mappers.ToDto(page);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid id)
    {
        var page = await _pages.GetByIdAsync(id);
        return page is not null && await UserOwnsSiteAsync(userId, page.SiteId) && await _pages.DeleteAsync(id);
    }

    private async Task<bool> UserOwnsSiteAsync(Guid userId, Guid siteId)
    {
        var site = await _sites.GetByIdAsync(siteId);
        return site is not null && site.UserId == userId;
    }
}
