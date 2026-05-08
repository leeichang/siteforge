using SiteForge.Core.Entities;

namespace SiteForge.Core.Interfaces.Repositories;

public interface RAssetRepository : IBaseRepository<Asset>
{
    Task<List<Asset>> GetBySiteIdAsync(Guid siteId);
    Task<List<Asset>> GetBySiteAndTypeAsync(Guid siteId, string mimeType);
}
