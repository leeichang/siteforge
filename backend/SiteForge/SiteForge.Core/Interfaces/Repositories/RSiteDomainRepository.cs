using SiteForge.Core.Entities;

namespace SiteForge.Core.Interfaces.Repositories;

public interface RSiteDomainRepository : IBaseRepository<SiteDomain>
{
    Task<List<SiteDomain>> GetBySiteIdAsync(Guid siteId);
    Task<SiteDomain?> GetPrimaryDomainAsync(Guid siteId);
    Task<SiteDomain?> GetByDomainAsync(string domain);
    Task<bool> DomainExistsAsync(string domain);
}
