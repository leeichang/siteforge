using SiteForge.Core.DTOs;

namespace SiteForge.Core.Interfaces.Services;

public interface SiteService
{
    Task<List<SiteDto>> GetByUserAsync(Guid userId);
    Task<SiteDto?> GetByIdAsync(Guid userId, Guid id);
    Task<SiteDto> CreateAsync(Guid userId, CreateSiteRequest request);
    Task<SiteDto?> UpdateAsync(Guid userId, Guid id, UpdateSiteRequest request);
    Task<bool> DeleteAsync(Guid userId, Guid id);
    Task<PublishTaskDto?> PublishAsync(Guid userId, Guid siteId, PublishRequest request);
}
