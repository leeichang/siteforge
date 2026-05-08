using SqlSugar;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;

namespace SiteForge.Infrastructure.Repositories;

public class LayoutRepository : BaseRepository<Layout>, RLayoutRepository
{
    public LayoutRepository(ISqlSugarClient db) : base(db) { }

    public Task<Layout?> GetWithZonesAsync(Guid id) =>
        _db.Queryable<Layout>().Where(x => x.Id == id && !x.IsDeleted).FirstAsync();

    public Task<List<Layout>> GetActiveLayoutsAsync() =>
        _db.Queryable<Layout>().Where(x => x.IsActive && !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();
}
