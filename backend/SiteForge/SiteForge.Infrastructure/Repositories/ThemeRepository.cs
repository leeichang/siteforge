using SqlSugar;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;

namespace SiteForge.Infrastructure.Repositories;

public class ThemeRepository : BaseRepository<Theme>, RThemeRepository
{
    public ThemeRepository(ISqlSugarClient db) : base(db) { }

    public Task<List<Theme>> GetSystemThemesAsync() =>
        _db.Queryable<Theme>().Where(x => x.IsSystem && !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();

    public Task<List<Theme>> GetCustomThemesAsync(Guid userId) =>
        _db.Queryable<Theme>().Where(x => !x.IsSystem && !x.IsDeleted).OrderBy(x => x.Name).ToListAsync();
}
