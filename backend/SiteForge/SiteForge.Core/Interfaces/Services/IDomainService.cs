using SiteForge.Core.DTOs;

namespace SiteForge.Core.Interfaces.Services;

public interface DomainService
{
    Task<List<SiteDomainDto>> GetBySiteAsync(Guid siteId);
    Task<SiteDomainDto> AddAsync(Guid siteId, AddDomainRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<SiteDomainDto?> VerifyAsync(Guid id);
}
