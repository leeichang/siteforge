using SiteForge.Core.Entities;

namespace SiteForge.Core.Interfaces.Repositories;

public interface RThemeRepository : IBaseRepository<Theme>
{
    Task<List<Theme>> GetSystemThemesAsync();
    Task<List<Theme>> GetCustomThemesAsync(Guid userId);
}
