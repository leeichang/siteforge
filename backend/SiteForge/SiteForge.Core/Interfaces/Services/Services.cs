using SiteForge.Core.DTOs;
using SiteForge.Core.Entities;

namespace SiteForge.Core.Interfaces.Services;

public interface AuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request);
    Task<UserDto?> GetProfileAsync(Guid userId);
    Task<UserDto?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
}

public interface SiteService
{
    Task<List<SiteDto>> GetByUserAsync(Guid userId);
    Task<SiteDto?> GetByIdAsync(Guid userId, Guid id);
    Task<SiteDto> CreateAsync(Guid userId, CreateSiteRequest request);
    Task<SiteDto?> UpdateAsync(Guid userId, Guid id, UpdateSiteRequest request);
    Task<bool> DeleteAsync(Guid userId, Guid id);
    Task<PublishTaskDto?> PublishAsync(Guid userId, Guid siteId, PublishRequest request);
}

public interface PageService
{
    Task<List<PageDto>> GetBySiteAsync(Guid userId, Guid siteId);
    Task<PageDetailDto?> GetByIdAsync(Guid userId, Guid id);
    Task<PageDto?> CreateAsync(Guid userId, Guid siteId, CreatePageRequest request);
    Task<PageDto?> UpdateAsync(Guid userId, Guid id, UpdatePageRequest request);
    Task<bool> DeleteAsync(Guid userId, Guid id);
}

public interface WidgetService
{
    Task<List<WidgetDto>> GetByPageAsync(Guid pageId);
    Task<WidgetDto> AddAsync(Guid pageId, AddWidgetRequest request);
    Task<WidgetDto?> UpdateAsync(Guid id, UpdateWidgetRequest request);
    Task<bool> DeleteAsync(Guid id);
}

public interface WidgetTemplateService
{
    Task<List<WidgetTemplateDto>> GetAllAsync(string? category = null);
    Task<WidgetTemplateDto?> GetByIdAsync(Guid id);
}

public interface ThemeService
{
    Task<List<ThemeDto>> GetSystemAsync();
    Task<ThemeDto?> GetByIdAsync(Guid id);
    Task<ThemeDto> CreateAsync(CreateThemeRequest request);
    Task<ThemeDto?> UpdateAsync(Guid id, UpdateThemeRequest request);
    Task<bool> DeleteAsync(Guid id);
}

public interface LayoutService
{
    Task<List<LayoutDto>> GetActiveAsync();
    Task<LayoutDto?> GetByIdAsync(Guid id);
    Task<LayoutDto> CreateAsync(CreateLayoutRequest request);
    Task<LayoutDto?> UpdateAsync(Guid id, UpdateLayoutRequest request);
    Task<bool> DeleteAsync(Guid id);
}

public interface DomainService
{
    Task<List<SiteDomainDto>> GetBySiteAsync(Guid siteId);
    Task<SiteDomainDto> AddAsync(Guid siteId, AddDomainRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<SiteDomainDto?> VerifyAsync(Guid id);
}

public interface AssetService
{
    Task<List<AssetDto>> GetBySiteAsync(Guid siteId);
    Task<AssetDto> CreateAsync(UploadAssetRequest request);
}

public interface AiConversationService
{
    Task<List<ConversationDto>> GetBySiteAsync(Guid siteId);
    Task<ConversationDto> CreateAsync(CreateConversationRequest request);
    Task<List<MessageDto>> GetMessagesAsync(Guid conversationId);
    Task<MessageDto> SendMessageAsync(Guid conversationId, SendMessageRequest request);
}

public interface PublishTaskService
{
    Task<List<PublishTaskDto>> GetBySiteAsync(Guid siteId);
    Task<PublishTaskDto?> GetLatestAsync(Guid siteId);
    Task<PublishTaskDto?> RetryAsync(Guid id);
}
