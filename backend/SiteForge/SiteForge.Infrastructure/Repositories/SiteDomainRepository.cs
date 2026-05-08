using SqlSugar;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;

namespace SiteForge.Infrastructure.Repositories;

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
