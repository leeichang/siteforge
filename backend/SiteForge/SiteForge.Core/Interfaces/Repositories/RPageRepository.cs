using SiteForge.Core.Entities;

namespace SiteForge.Core.Interfaces.Repositories;

public interface RPageRepository : IBaseRepository<Page>
{
    Task<List<Page>> GetBySiteIdAsync(Guid siteId);
    Task<Page?> GetBySiteAndSlugAsync(Guid siteId, string slug);
    Task<Page?> GetHomePageAsync(Guid siteId);
    Task<List<Page>> GetSitePagesWithWidgetsAsync(Guid siteId);
}
