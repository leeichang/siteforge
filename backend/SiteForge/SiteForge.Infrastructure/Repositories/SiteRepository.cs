using SqlSugar;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;

namespace SiteForge.Infrastructure.Repositories;

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
