using SiteForge.Core.DTOs;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;
using SiteForge.Core.Interfaces.Services;
using SiteForge.Core.Utilities;

namespace SiteForge.Core.Services;

internal static class Mappers
{
    public static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        DisplayName = user.DisplayName ?? user.Email,
        AvatarUrl = user.AvatarUrl,
        Role = user.Role
    };

    public static SiteDto ToDto(Site site) => new()
    {
        Id = site.Id,
        Name = site.Name,
        Description = site.Description,
        LogoUrl = site.LogoUrl,
        FaviconUrl = site.FaviconUrl,
        Slug = site.Slug,
        Status = site.Status,
        PublishedUrl = site.PublishedUrl,
        CreatedAt = site.CreatedAt,
        UpdatedAt = site.UpdatedAt
    };

    public static LinkDto ToLinkDto(Page page) => new()
    {
        Id = page.Id,
        Title = page.Title,
        Slug = page.Slug,
        IsHomePage = page.IsHome,
        DisplayOrder = page.DisplayOrder
    };

    public static PageDto ToDto(Page page) => new()
    {
        Id = page.Id,
        SiteId = page.SiteId,
        Title = page.Title,
        Slug = page.Slug,
        PageType = page.PageType,
        IsHome = page.IsHome,
        IsPublished = page.IsPublished,
        DisplayOrder = page.DisplayOrder,
        ShowInNav = page.ShowInNav,
        MetaTitle = page.MetaTitle,
        MetaDescription = page.MetaDescription,
        CreatedAt = page.CreatedAt,
        UpdatedAt = page.UpdatedAt,
        PublishedAt = page.PublishedAt
    };

    public static PageDetailDto ToDetailDto(Page page) => new()
    {
        Id = page.Id,
        SiteId = page.SiteId,
        Title = page.Title,
        Slug = page.Slug,
        PageType = page.PageType,
        IsHome = page.IsHome,
        IsPublished = page.IsPublished,
        DisplayOrder = page.DisplayOrder,
        ShowInNav = page.ShowInNav,
        MetaTitle = page.MetaTitle,
        MetaDescription = page.MetaDescription,
        CreatedAt = page.CreatedAt,
        UpdatedAt = page.UpdatedAt,
        PublishedAt = page.PublishedAt,
        Components = page.Components,
        Styles = page.Styles,
        HtmlContent = page.HtmlContent,
        CssContent = page.CssContent,
        JsContent = page.JsContent
    };

    public static WidgetDto ToDto(WidgetBase widget) => new()
    {
        Id = widget.Id,
        TemplateId = widget.TemplateId,
        TemplateName = widget.Template?.Name ?? string.Empty,
        Title = widget.Title,
        Content = widget.Content,
        Style = widget.Style,
        ZoneName = widget.ZoneName,
        Order = widget.Order,
        IsHidden = widget.IsHidden
    };

    public static WidgetTemplateDto ToDto(WidgetTemplate template) => new()
    {
        Id = template.Id,
        Name = template.Name,
        Category = template.Category,
        ThumbnailUrl = template.ThumbnailUrl,
        Description = template.Description,
        DefaultContent = template.DefaultContent,
        DefaultStyle = template.DefaultStyle,
        EditableProps = template.EditableProps,
        IsSystem = template.IsSystem,
        IsActive = template.IsActive,
        DisplayOrder = template.DisplayOrder,
        CreatedAt = template.CreatedAt
    };

    public static ThemeDto ToDto(Theme theme) => new()
    {
        Id = theme.Id,
        Name = theme.Name,
        Description = theme.Description,
        ThumbnailUrl = theme.ThumbnailUrl,
        IsSystem = theme.IsSystem,
        Colors = theme.Colors,
        Fonts = theme.Fonts,
        FontImportUrl = theme.FontImportUrl,
        Spacing = theme.Spacing,
        BorderRadius = theme.BorderRadius,
        Shadows = theme.Shadows,
        CustomCss = theme.CustomCss,
        Version = theme.Version,
        CreatedAt = theme.CreatedAt
    };

    public static LayoutDto ToDto(Layout layout) => new()
    {
        Id = layout.Id,
        Name = layout.Name,
        Description = layout.Description,
        ThumbnailUrl = layout.ThumbnailUrl,
        BodyHtml = layout.BodyHtml,
        IsSystem = layout.IsSystem,
        IsActive = layout.IsActive,
        CreatedAt = layout.CreatedAt
    };

    public static LayoutZoneDto ToDto(LayoutZone zone) => new()
    {
        Id = zone.Id,
        Name = zone.Name,
        Title = zone.Title,
        Width = zone.Width,
        Order = zone.Order,
        CssClass = zone.CssClass,
        PlaceholderHtml = zone.PlaceholderHtml,
        IsEditable = zone.IsEditable
    };

    public static SiteDomainDto ToDto(SiteDomain domain) => new()
    {
        Id = domain.Id,
        Domain = domain.Domain,
        IsPrimary = domain.IsPrimary,
        IsVerified = domain.IsVerified
    };

    public static AssetDto ToDto(Asset asset) => new()
    {
        Id = asset.Id,
        SiteId = asset.SiteId,
        FileName = asset.FileName,
        MimeType = asset.MimeType,
        FileSize = asset.FileSize,
        PublicUrl = asset.PublicUrl,
        Width = asset.Width,
        Height = asset.Height,
        AltText = asset.AltText,
        Source = asset.Source,
        CreatedAt = asset.CreatedAt
    };

    public static ConversationDto ToDto(AiConversation conversation) => new()
    {
        Id = conversation.Id,
        SiteId = conversation.SiteId,
        PageId = conversation.PageId,
        Title = conversation.Title,
        Summary = conversation.Summary,
        Model = conversation.Model,
        MessageCount = conversation.MessageCount,
        IsCompleted = conversation.IsCompleted,
        LastActivityAt = conversation.LastActivityAt,
        CreatedAt = conversation.CreatedAt
    };

    public static MessageDto ToDto(AiMessage message) => new()
    {
        Id = message.Id,
        ConversationId = message.ConversationId,
        Role = message.Role,
        Content = message.Content,
        ActionType = message.ActionType,
        ActionResult = message.ActionResult,
        Metadata = message.Metadata,
        ClientTimestamp = message.ClientTimestamp,
        CreatedAt = message.CreatedAt
    };

    public static PublishTaskDto ToDto(PublishTask task) => new()
    {
        Id = task.Id,
        SiteId = task.SiteId,
        TaskType = task.TaskType,
        Status = task.Status,
        ErrorMessage = task.ErrorMessage,
        TotalPages = task.TotalPages,
        PublishedPages = task.PublishedPages,
        TargetUrl = task.TargetUrl,
        StartedAt = task.StartedAt,
        CompletedAt = task.CompletedAt,
        CreatedAt = task.CreatedAt
    };

    public static string Slugify(string value)
    {
        var chars = value.Trim().ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray();
        var slug = string.Join("-", new string(chars).Split('-', StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N")[..8] : slug;
    }
}

