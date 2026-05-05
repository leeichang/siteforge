using SiteForge.Core.DTOs;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;
using SiteForge.Core.Interfaces.Services;
using SiteForge.Core.Utilities;

namespace SiteForge.Core.Services;

public class AuthServiceImpl : AuthService
{
    private readonly RUserRepository _users;
    private readonly PasswordHelper _passwords;
    private readonly JwtHelper _jwt;

    public AuthServiceImpl(RUserRepository users, PasswordHelper passwords, JwtHelper jwt)
    {
        _users = users;
        _passwords = passwords;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = NormalizeEmail(request.Email);
        if (await _users.EmailExistsAsync(email))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var user = await _users.AddAsync(new User
        {
            Email = email,
            Username = email,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? email : request.DisplayName.Trim(),
            PasswordHash = _passwords.HashPassword(request.Password),
            Role = "user",
            RefreshToken = _jwt.GenerateRefreshToken(),
            RefreshTokenExpiry = _jwt.GetRefreshTokenExpiry()
        });

        await _users.UpdateAsync(user);
        return ToAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _users.GetByEmailAsync(email);
        if (user is null || !_passwords.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid email or password.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.RefreshToken = _jwt.GenerateRefreshToken();
        user.RefreshTokenExpiry = _jwt.GetRefreshTokenExpiry();
        await _users.UpdateAsync(user);

        return ToAuthResponse(user);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request)
    {
        var users = await _users.GetAllAsync();
        var user = users.FirstOrDefault(x => x.RefreshToken == request.RefreshToken && x.RefreshTokenExpiry > DateTime.UtcNow);
        if (user is null)
        {
            throw new InvalidOperationException("Invalid refresh token.");
        }

        user.RefreshToken = _jwt.GenerateRefreshToken();
        user.RefreshTokenExpiry = _jwt.GetRefreshTokenExpiry();
        await _users.UpdateAsync(user);

        return ToAuthResponse(user);
    }

    public async Task<UserDto?> GetProfileAsync(Guid userId)
    {
        var user = await _users.GetByIdAsync(userId);
        return user is null ? null : Mappers.ToDto(user);
    }

    public async Task<UserDto?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _users.GetByIdAsync(userId);
        if (user is null) return null;

        if (request.DisplayName is not null) user.DisplayName = request.DisplayName.Trim();
        if (request.AvatarUrl is not null) user.AvatarUrl = request.AvatarUrl.Trim();
        await _users.UpdateAsync(user);
        return Mappers.ToDto(user);
    }

    private AuthResponse ToAuthResponse(User user) => new()
    {
        Token = _jwt.GenerateAccessToken(user),
        RefreshToken = user.RefreshToken ?? string.Empty,
        ExpiresAt = _jwt.GetAccessTokenExpiry(),
        User = Mappers.ToDto(user)
    };

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}

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

        var pageCount = (await _pages.GetBySiteIdAsync(siteId)).Count;
        var task = await _publishTasks.AddAsync(new PublishTask
        {
            SiteId = siteId,
            TaskType = request.TaskType,
            Status = "pending",
            TotalPages = pageCount,
            TargetUrl = request.TargetUrl
        });
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
}

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

public class WidgetServiceImpl : WidgetService
{
    private readonly RWidgetBaseRepository _widgets;
    private readonly RWidgetTemplateRepository _templates;

    public WidgetServiceImpl(RWidgetBaseRepository widgets, RWidgetTemplateRepository templates)
    {
        _widgets = widgets;
        _templates = templates;
    }

    public async Task<List<WidgetDto>> GetByPageAsync(Guid pageId) =>
        (await _widgets.GetByPageIdAsync(pageId)).Select(Mappers.ToDto).ToList();

    public async Task<WidgetDto> AddAsync(Guid pageId, AddWidgetRequest request)
    {
        var template = await _templates.GetByIdAsync(request.TemplateId);
        var widget = await _widgets.AddAsync(new WidgetBase
        {
            PageId = pageId,
            TemplateId = request.TemplateId,
            Title = template?.Name ?? "Widget",
            Content = request.Content ?? template?.EditableProps ?? "{}",
            ZoneName = request.ZoneName,
            Order = request.Order
        });
        return Mappers.ToDto(widget);
    }

    public async Task<WidgetDto?> UpdateAsync(Guid id, UpdateWidgetRequest request)
    {
        var widget = await _widgets.GetByIdAsync(id);
        if (widget is null) return null;

        if (request.Title is not null) widget.Title = request.Title;
        if (request.Content is not null) widget.Content = request.Content;
        if (request.Style is not null) widget.Style = request.Style;
        if (request.ZoneName is not null) widget.ZoneName = request.ZoneName;
        if (request.Order.HasValue) widget.Order = request.Order.Value;
        if (request.CustomCssClass is not null) widget.CustomCssClass = request.CustomCssClass;
        if (request.IsHidden.HasValue) widget.IsHidden = request.IsHidden.Value;

        await _widgets.UpdateAsync(widget);
        return Mappers.ToDto(widget);
    }

    public Task<bool> DeleteAsync(Guid id) => _widgets.DeleteAsync(id);
}

public class WidgetTemplateServiceImpl : WidgetTemplateService
{
    private readonly RWidgetTemplateRepository _templates;

    public WidgetTemplateServiceImpl(RWidgetTemplateRepository templates)
    {
        _templates = templates;
    }

    public async Task<List<WidgetTemplateDto>> GetAllAsync(string? category = null)
    {
        var templates = string.IsNullOrWhiteSpace(category)
            ? await _templates.GetActiveTemplatesAsync()
            : await _templates.GetByCategoryAsync(category);
        return templates.Select(Mappers.ToDto).ToList();
    }

    public async Task<WidgetTemplateDto?> GetByIdAsync(Guid id)
    {
        var template = await _templates.GetByIdAsync(id);
        return template is null ? null : Mappers.ToDto(template);
    }
}

public class ThemeServiceImpl : ThemeService
{
    private readonly RThemeRepository _themes;

    public ThemeServiceImpl(RThemeRepository themes)
    {
        _themes = themes;
    }

    public async Task<List<ThemeDto>> GetSystemAsync() =>
        (await _themes.GetSystemThemesAsync()).Select(Mappers.ToDto).ToList();

    public async Task<ThemeDto?> GetByIdAsync(Guid id)
    {
        var theme = await _themes.GetByIdAsync(id);
        return theme is null ? null : Mappers.ToDto(theme);
    }

    public async Task<ThemeDto> CreateAsync(CreateThemeRequest request)
    {
        var theme = await _themes.AddAsync(new Theme
        {
            Name = request.Name,
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            Colors = request.Colors,
            Fonts = request.Fonts,
            FontImportUrl = request.FontImportUrl,
            Spacing = request.Spacing,
            BorderRadius = request.BorderRadius,
            Shadows = request.Shadows,
            CustomCss = request.CustomCss,
            IsSystem = false
        });
        return Mappers.ToDto(theme);
    }

    public async Task<ThemeDto?> UpdateAsync(Guid id, UpdateThemeRequest request)
    {
        var theme = await _themes.GetByIdAsync(id);
        if (theme is null) return null;

        if (request.Name is not null) theme.Name = request.Name;
        if (request.Description is not null) theme.Description = request.Description;
        if (request.ThumbnailUrl is not null) theme.ThumbnailUrl = request.ThumbnailUrl;
        if (request.Colors is not null) theme.Colors = request.Colors;
        if (request.Fonts is not null) theme.Fonts = request.Fonts;
        if (request.FontImportUrl is not null) theme.FontImportUrl = request.FontImportUrl;
        if (request.Spacing is not null) theme.Spacing = request.Spacing;
        if (request.BorderRadius is not null) theme.BorderRadius = request.BorderRadius;
        if (request.Shadows is not null) theme.Shadows = request.Shadows;
        if (request.CustomCss is not null) theme.CustomCss = request.CustomCss;

        await _themes.UpdateAsync(theme);
        return Mappers.ToDto(theme);
    }

    public Task<bool> DeleteAsync(Guid id) => _themes.DeleteAsync(id);
}

public class LayoutServiceImpl : LayoutService
{
    private readonly RLayoutRepository _layouts;
    private readonly RLayoutZoneRepository _zones;

    public LayoutServiceImpl(RLayoutRepository layouts, RLayoutZoneRepository zones)
    {
        _layouts = layouts;
        _zones = zones;
    }

    public async Task<List<LayoutDto>> GetActiveAsync()
    {
        var layouts = await _layouts.GetActiveLayoutsAsync();
        var result = new List<LayoutDto>();
        foreach (var layout in layouts)
        {
            result.Add(await ToLayoutDtoAsync(layout));
        }
        return result;
    }

    public async Task<LayoutDto?> GetByIdAsync(Guid id)
    {
        var layout = await _layouts.GetByIdAsync(id);
        return layout is null ? null : await ToLayoutDtoAsync(layout);
    }

    public async Task<LayoutDto> CreateAsync(CreateLayoutRequest request)
    {
        var layout = await _layouts.AddAsync(new Layout
        {
            Name = request.Name,
            Description = request.Description,
            BodyHtml = request.BodyHtml,
            IsSystem = false,
            IsActive = true
        });

        if (request.Zones.Count > 0)
        {
            await _zones.AddRangeAsync(request.Zones.Select(x => new LayoutZone
            {
                LayoutId = layout.Id,
                Name = x.Name,
                Title = x.Title,
                Width = x.Width,
                Order = x.Order,
                CssClass = x.CssClass,
                PlaceholderHtml = x.PlaceholderHtml,
                IsEditable = x.IsEditable
            }).ToList());
        }

        return await ToLayoutDtoAsync(layout);
    }

    public async Task<LayoutDto?> UpdateAsync(Guid id, UpdateLayoutRequest request)
    {
        var layout = await _layouts.GetByIdAsync(id);
        if (layout is null) return null;

        if (request.Name is not null) layout.Name = request.Name;
        if (request.Description is not null) layout.Description = request.Description;
        if (request.BodyHtml is not null) layout.BodyHtml = request.BodyHtml;
        await _layouts.UpdateAsync(layout);

        return await ToLayoutDtoAsync(layout);
    }

    public Task<bool> DeleteAsync(Guid id) => _layouts.DeleteAsync(id);

    private async Task<LayoutDto> ToLayoutDtoAsync(Layout layout)
    {
        var dto = Mappers.ToDto(layout);
        dto.Zones = (await _zones.GetByLayoutIdAsync(layout.Id)).Select(Mappers.ToDto).ToList();
        return dto;
    }
}

public class DomainServiceImpl : DomainService
{
    private readonly RSiteDomainRepository _domains;

    public DomainServiceImpl(RSiteDomainRepository domains)
    {
        _domains = domains;
    }

    public async Task<List<SiteDomainDto>> GetBySiteAsync(Guid siteId) =>
        (await _domains.GetBySiteIdAsync(siteId)).Select(Mappers.ToDto).ToList();

    public async Task<SiteDomainDto> AddAsync(Guid siteId, AddDomainRequest request)
    {
        if (await _domains.DomainExistsAsync(request.Domain))
        {
            throw new InvalidOperationException("Domain already exists.");
        }

        var domain = await _domains.AddAsync(new SiteDomain
        {
            SiteId = siteId,
            Domain = request.Domain.Trim().ToLowerInvariant(),
            IsPrimary = request.IsPrimary,
            VerificationToken = Guid.NewGuid().ToString("N"),
            DnsStatus = "pending"
        });
        return Mappers.ToDto(domain);
    }

    public Task<bool> DeleteAsync(Guid id) => _domains.DeleteAsync(id);

    public async Task<SiteDomainDto?> VerifyAsync(Guid id)
    {
        var domain = await _domains.GetByIdAsync(id);
        if (domain is null) return null;

        domain.IsVerified = true;
        domain.DnsStatus = "configured";
        await _domains.UpdateAsync(domain);
        return Mappers.ToDto(domain);
    }
}

public class AssetServiceImpl : AssetService
{
    private readonly RAssetRepository _assets;

    public AssetServiceImpl(RAssetRepository assets)
    {
        _assets = assets;
    }

    public async Task<List<AssetDto>> GetBySiteAsync(Guid siteId) =>
        (await _assets.GetBySiteIdAsync(siteId)).Select(Mappers.ToDto).ToList();

    public async Task<AssetDto> CreateAsync(UploadAssetRequest request)
    {
        var asset = await _assets.AddAsync(new Asset
        {
            SiteId = request.SiteId,
            FileName = request.FileName,
            MimeType = request.MimeType,
            FileSize = request.FileSize,
            StoragePath = request.StoragePath,
            PublicUrl = request.PublicUrl,
            Width = request.Width,
            Height = request.Height,
            AltText = request.AltText,
            Source = request.Source
        });
        return Mappers.ToDto(asset);
    }
}

public class AiConversationServiceImpl : AiConversationService
{
    private readonly RAiConversationRepository _conversations;
    private readonly RAiMessageRepository _messages;

    public AiConversationServiceImpl(RAiConversationRepository conversations, RAiMessageRepository messages)
    {
        _conversations = conversations;
        _messages = messages;
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
}

public class PublishTaskServiceImpl : PublishTaskService
{
    private readonly RPublishTaskRepository _tasks;

    public PublishTaskServiceImpl(RPublishTaskRepository tasks)
    {
        _tasks = tasks;
    }

    public async Task<List<PublishTaskDto>> GetBySiteAsync(Guid siteId) =>
        (await _tasks.GetBySiteIdAsync(siteId)).Select(Mappers.ToDto).ToList();

    public async Task<PublishTaskDto?> GetLatestAsync(Guid siteId)
    {
        var task = await _tasks.GetLatestTaskAsync(siteId);
        return task is null ? null : Mappers.ToDto(task);
    }

    public async Task<PublishTaskDto?> RetryAsync(Guid id)
    {
        var task = await _tasks.GetByIdAsync(id);
        if (task is null) return null;

        task.Status = "pending";
        task.ErrorMessage = null;
        task.StartedAt = null;
        task.CompletedAt = null;
        await _tasks.UpdateAsync(task);
        return Mappers.ToDto(task);
    }
}

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
