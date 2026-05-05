using SqlSugar;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;

namespace SiteForge.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, RUserRepository
{
    public UserRepository(ISqlSugarClient db) : base(db) { }

    public Task<User?> GetByEmailAsync(string email) =>
        _db.Queryable<User>().Where(x => x.Email == email && !x.IsDeleted).FirstAsync();

    public Task<bool> EmailExistsAsync(string email) =>
        _db.Queryable<User>().AnyAsync(x => x.Email == email && !x.IsDeleted);
}

public class SiteRepository : BaseRepository<Site>, RSiteRepository
{
    public SiteRepository(ISqlSugarClient db) : base(db) { }

    public Task<List<Site>> GetByUserIdAsync(Guid userId) =>
        _db.Queryable<Site>().Where(x => x.UserId == userId && !x.IsDeleted).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<Site?> GetBySlugAsync(string slug) =>
        _db.Queryable<Site>().Where(x => x.Slug == slug && !x.IsDeleted).FirstAsync();

    public Task<int> GetUserSiteCountAsync(Guid userId) =>
        _db.Queryable<Site>().Where(x => x.UserId == userId && !x.IsDeleted).CountAsync();
}

public class PageRepository : BaseRepository<Page>, RPageRepository
{
    public PageRepository(ISqlSugarClient db) : base(db) { }

    public Task<List<Page>> GetBySiteIdAsync(Guid siteId) =>
        _db.Queryable<Page>().Where(x => x.SiteId == siteId && !x.IsDeleted).OrderBy(x => x.DisplayOrder).ToListAsync();

    public Task<Page?> GetBySiteAndSlugAsync(Guid siteId, string slug) =>
        _db.Queryable<Page>().Where(x => x.SiteId == siteId && x.Slug == slug && !x.IsDeleted).FirstAsync();

    public Task<Page?> GetHomePageAsync(Guid siteId) =>
        _db.Queryable<Page>().Where(x => x.SiteId == siteId && x.IsHome && !x.IsDeleted).FirstAsync();

    public Task<List<Page>> GetSitePagesWithWidgetsAsync(Guid siteId) =>
        GetBySiteIdAsync(siteId);
}

public class WidgetTemplateRepository : BaseRepository<WidgetTemplate>, RWidgetTemplateRepository
{
    public WidgetTemplateRepository(ISqlSugarClient db) : base(db) { }

    public Task<List<WidgetTemplate>> GetByCategoryAsync(string category) =>
        _db.Queryable<WidgetTemplate>().Where(x => x.Category == category && x.IsActive && !x.IsDeleted).OrderBy(x => x.DisplayOrder).ToListAsync();

    public Task<List<WidgetTemplate>> GetActiveTemplatesAsync() =>
        _db.Queryable<WidgetTemplate>().Where(x => x.IsActive && !x.IsDeleted).OrderBy(x => x.DisplayOrder).ToListAsync();
}

public class WidgetBaseRepository : BaseRepository<WidgetBase>, RWidgetBaseRepository
{
    public WidgetBaseRepository(ISqlSugarClient db) : base(db) { }

    public Task<List<WidgetBase>> GetByPageIdAsync(Guid pageId) =>
        _db.Queryable<WidgetBase>().Where(x => x.PageId == pageId && !x.IsDeleted).OrderBy(x => x.Order).ToListAsync();

    public Task<List<WidgetBase>> GetByPageAndZoneAsync(Guid pageId, string zoneName) =>
        _db.Queryable<WidgetBase>().Where(x => x.PageId == pageId && x.ZoneName == zoneName && !x.IsDeleted).OrderBy(x => x.Order).ToListAsync();

    public async Task<bool> ReorderAsync(Guid widgetId, int newOrder) =>
        await _db.Updateable<WidgetBase>()
            .SetColumns(x => x.Order, newOrder)
            .SetColumns(x => x.UpdatedAt, DateTime.UtcNow)
            .Where(x => x.Id == widgetId)
            .ExecuteCommandAsync() > 0;
}

public class ThemeRepository : BaseRepository<Theme>, RThemeRepository
{
    public ThemeRepository(ISqlSugarClient db) : base(db) { }

    public Task<List<Theme>> GetSystemThemesAsync() =>
        _db.Queryable<Theme>().Where(x => x.IsSystem && !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();

    public Task<List<Theme>> GetCustomThemesAsync(Guid userId) =>
        _db.Queryable<Theme>().Where(x => !x.IsSystem && !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();
}

public class LayoutRepository : BaseRepository<Layout>, RLayoutRepository
{
    public LayoutRepository(ISqlSugarClient db) : base(db) { }

    public Task<Layout?> GetWithZonesAsync(Guid id) =>
        _db.Queryable<Layout>().Where(x => x.Id == id && !x.IsDeleted).FirstAsync();

    public Task<List<Layout>> GetActiveLayoutsAsync() =>
        _db.Queryable<Layout>().Where(x => x.IsActive && !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();
}

public class LayoutZoneRepository : BaseRepository<LayoutZone>, RLayoutZoneRepository
{
    public LayoutZoneRepository(ISqlSugarClient db) : base(db) { }

    public Task<List<LayoutZone>> GetByLayoutIdAsync(Guid layoutId) =>
        _db.Queryable<LayoutZone>().Where(x => x.LayoutId == layoutId && !x.IsDeleted).OrderBy(x => x.Order).ToListAsync();
}

public class SiteDomainRepository : BaseRepository<SiteDomain>, RSiteDomainRepository
{
    public SiteDomainRepository(ISqlSugarClient db) : base(db) { }

    public Task<List<SiteDomain>> GetBySiteIdAsync(Guid siteId) =>
        _db.Queryable<SiteDomain>().Where(x => x.SiteId == siteId && !x.IsDeleted).OrderByDescending(x => x.IsPrimary).ToListAsync();

    public Task<SiteDomain?> GetPrimaryDomainAsync(Guid siteId) =>
        _db.Queryable<SiteDomain>().Where(x => x.SiteId == siteId && x.IsPrimary && !x.IsDeleted).FirstAsync();

    public Task<SiteDomain?> GetByDomainAsync(string domain) =>
        _db.Queryable<SiteDomain>().Where(x => x.Domain == domain && !x.IsDeleted).FirstAsync();

    public Task<bool> DomainExistsAsync(string domain) =>
        _db.Queryable<SiteDomain>().AnyAsync(x => x.Domain == domain && !x.IsDeleted);
}

public class AiConversationRepository : BaseRepository<AiConversation>, RAiConversationRepository
{
    public AiConversationRepository(ISqlSugarClient db) : base(db) { }

    public Task<List<AiConversation>> GetBySiteIdAsync(Guid siteId) =>
        _db.Queryable<AiConversation>().Where(x => x.SiteId == siteId && !x.IsDeleted).OrderByDescending(x => x.LastActivityAt).ToListAsync();

    public Task<List<AiConversation>> GetByPageIdAsync(Guid pageId) =>
        _db.Queryable<AiConversation>().Where(x => x.PageId == pageId && !x.IsDeleted).OrderByDescending(x => x.LastActivityAt).ToListAsync();
}

public class AiMessageRepository : BaseRepository<AiMessage>, RAiMessageRepository
{
    public AiMessageRepository(ISqlSugarClient db) : base(db) { }

    public Task<List<AiMessage>> GetByConversationIdAsync(Guid conversationId) =>
        _db.Queryable<AiMessage>().Where(x => x.ConversationId == conversationId && !x.IsDeleted).OrderBy(x => x.CreatedAt).ToListAsync();

    public Task<AiMessage?> GetLastAssistantMessageAsync(Guid conversationId) =>
        _db.Queryable<AiMessage>().Where(x => x.ConversationId == conversationId && x.Role == "assistant" && !x.IsDeleted).OrderByDescending(x => x.CreatedAt).FirstAsync();
}

public class AssetRepository : BaseRepository<Asset>, RAssetRepository
{
    public AssetRepository(ISqlSugarClient db) : base(db) { }

    public Task<List<Asset>> GetBySiteIdAsync(Guid siteId) =>
        _db.Queryable<Asset>().Where(x => x.SiteId == siteId && !x.IsDeleted).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<List<Asset>> GetBySiteAndTypeAsync(Guid siteId, string mimeType) =>
        _db.Queryable<Asset>().Where(x => x.SiteId == siteId && x.MimeType == mimeType && !x.IsDeleted).OrderByDescending(x => x.CreatedAt).ToListAsync();
}

public class PublishTaskRepository : BaseRepository<PublishTask>, RPublishTaskRepository
{
    public PublishTaskRepository(ISqlSugarClient db) : base(db) { }

    public Task<List<PublishTask>> GetBySiteIdAsync(Guid siteId) =>
        _db.Queryable<PublishTask>().Where(x => x.SiteId == siteId && !x.IsDeleted).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<PublishTask?> GetLatestTaskAsync(Guid siteId) =>
        _db.Queryable<PublishTask>().Where(x => x.SiteId == siteId && !x.IsDeleted).OrderByDescending(x => x.CreatedAt).FirstAsync();

    public Task<List<PublishTask>> GetPendingTasksAsync() =>
        _db.Queryable<PublishTask>().Where(x => x.Status == "pending" && !x.IsDeleted).OrderBy(x => x.CreatedAt).ToListAsync();
}
