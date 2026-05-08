using SqlSugar;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;

namespace SiteForge.Infrastructure.Repositories;

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
