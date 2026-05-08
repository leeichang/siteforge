using SqlSugar;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;

namespace SiteForge.Infrastructure.Repositories;

public class WidgetTemplateRepository : BaseRepository<WidgetTemplate>, RWidgetTemplateRepository
{
    public WidgetTemplateRepository(ISqlSugarClient db) : base(db) { }

    public Task<List<WidgetTemplate>> GetByCategoryAsync(string category) =>
        _db.Queryable<WidgetTemplate>().Where(x => x.Category == category && x.IsActive && !x.IsDeleted).OrderBy(x => x.DisplayOrder).ToListAsync();

    public Task<List<WidgetTemplate>> GetActiveTemplatesAsync() =>
        _db.Queryable<WidgetTemplate>().Where(x => x.IsActive && !x.IsDeleted).OrderBy(x => x.DisplayOrder).ToListAsync();
}
