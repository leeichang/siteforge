using SqlSugar;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;

namespace SiteForge.Infrastructure.Repositories;

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
