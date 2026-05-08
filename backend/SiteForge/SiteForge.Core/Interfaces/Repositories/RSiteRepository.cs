using SiteForge.Core.Entities;

namespace SiteForge.Core.Interfaces.Repositories;

public interface RSiteRepository : IBaseRepository<Site>
{
    Task<List<Site>> GetByUserIdAsync(Guid userId);
    Task<Site?> GetBySlugAsync(string slug);
    Task<int> GetUserSiteCountAsync(Guid userId);
}
