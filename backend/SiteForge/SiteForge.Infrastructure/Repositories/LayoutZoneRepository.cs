using SqlSugar;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;

namespace SiteForge.Infrastructure.Repositories;

public class LayoutZoneRepository : BaseRepository<LayoutZone>, RLayoutZoneRepository
{
    public LayoutZoneRepository(ISqlSugarClient db) : base(db) { }

    public Task<List<LayoutZone>> GetByLayoutIdAsync(Guid layoutId) =>
        _db.Queryable<LayoutZone>().Where(x => x.LayoutId == layoutId && !x.IsDeleted).OrderBy(x => x.Order).ToListAsync();
}
