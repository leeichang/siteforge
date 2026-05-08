using SiteForge.Core.Entities;

namespace SiteForge.Core.Interfaces.Repositories;

public interface RLayoutZoneRepository : IBaseRepository<LayoutZone>
{
    Task<List<LayoutZone>> GetByLayoutIdAsync(Guid layoutId);
}
