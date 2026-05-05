using SiteForge.Core.Entities;

namespace SiteForge.Core.Interfaces.Repositories;

/// <summary>
/// 基底 Repository 介面
/// </summary>
public interface IBaseRepository<T> where T : BaseEntity, new()
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync(bool includeDeleted = false);
    Task<List<T>> GetPagedAsync(int page, int pageSize, bool includeDeleted = false);
    Task<int> GetCountAsync(bool includeDeleted = false);
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(List<T> entities);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> HardDeleteAsync(Guid id);
    Task<bool> RestoreAsync(Guid id);
}

/// <summary>
/// 使用者 Repository
/// </summary>
public interface RUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
}

/// <summary>
/// 網站 Repository
/// </summary>
public interface RSiteRepository : IBaseRepository<Site>
{
    Task<List<Site>> GetByUserIdAsync(Guid userId);
    Task<Site?> GetBySlugAsync(string slug);
    Task<int> GetUserSiteCountAsync(Guid userId);
}

/// <summary>
/// 頁面 Repository
/// </summary>
public interface RPageRepository : IBaseRepository<Page>
{
    Task<List<Page>> GetBySiteIdAsync(Guid siteId);
    Task<Page?> GetBySiteAndSlugAsync(Guid siteId, string slug);
    Task<Page?> GetHomePageAsync(Guid siteId);
    Task<List<Page>> GetSitePagesWithWidgetsAsync(Guid siteId);
}

/// <summary>
/// 區塊模板 Repository
/// </summary>
public interface RWidgetTemplateRepository : IBaseRepository<WidgetTemplate>
{
    Task<List<WidgetTemplate>> GetByCategoryAsync(string category);
    Task<List<WidgetTemplate>> GetActiveTemplatesAsync();
}

/// <summary>
/// 區塊實例 Repository
/// </summary>
public interface RWidgetBaseRepository : IBaseRepository<WidgetBase>
{
    Task<List<WidgetBase>> GetByPageIdAsync(Guid pageId);
    Task<List<WidgetBase>> GetByPageAndZoneAsync(Guid pageId, string zoneName);
    Task<bool> ReorderAsync(Guid widgetId, int newOrder);
}

/// <summary>
/// 主題 Repository
/// </summary>
public interface RThemeRepository : IBaseRepository<Theme>
{
    Task<List<Theme>> GetSystemThemesAsync();
    Task<List<Theme>> GetCustomThemesAsync(Guid userId);
}

/// <summary>
/// 佈局 Repository
/// </summary>
public interface RLayoutRepository : IBaseRepository<Layout>
{
    Task<Layout?> GetWithZonesAsync(Guid id);
    Task<List<Layout>> GetActiveLayoutsAsync();
}

/// <summary>
/// 佈局區域 Repository
/// </summary>
public interface RLayoutZoneRepository : IBaseRepository<LayoutZone>
{
    Task<List<LayoutZone>> GetByLayoutIdAsync(Guid layoutId);
}

/// <summary>
/// 網站域名 Repository
/// </summary>
public interface RSiteDomainRepository : IBaseRepository<SiteDomain>
{
    Task<List<SiteDomain>> GetBySiteIdAsync(Guid siteId);
    Task<SiteDomain?> GetPrimaryDomainAsync(Guid siteId);
    Task<SiteDomain?> GetByDomainAsync(string domain);
    Task<bool> DomainExistsAsync(string domain);
}

/// <summary>
/// AI 對話 Repository
/// </summary>
public interface RAiConversationRepository : IBaseRepository<AiConversation>
{
    Task<List<AiConversation>> GetBySiteIdAsync(Guid siteId);
    Task<List<AiConversation>> GetByPageIdAsync(Guid pageId);
}

/// <summary>
/// AI 訊息 Repository
/// </summary>
public interface RAiMessageRepository : IBaseRepository<AiMessage>
{
    Task<List<AiMessage>> GetByConversationIdAsync(Guid conversationId);
    Task<AiMessage?> GetLastAssistantMessageAsync(Guid conversationId);
}

/// <summary>
/// 素材資源 Repository
/// </summary>
public interface RAssetRepository : IBaseRepository<Asset>
{
    Task<List<Asset>> GetBySiteIdAsync(Guid siteId);
    Task<List<Asset>> GetBySiteAndTypeAsync(Guid siteId, string mimeType);
}

/// <summary>
/// 發佈任務 Repository
/// </summary>
public interface RPublishTaskRepository : IBaseRepository<PublishTask>
{
    Task<List<PublishTask>> GetBySiteIdAsync(Guid siteId);
    Task<PublishTask?> GetLatestTaskAsync(Guid siteId);
    Task<List<PublishTask>> GetPendingTasksAsync();
}
