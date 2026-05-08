using SiteForge.Core.DTOs;

namespace SiteForge.Core.Interfaces.Services;

public interface PageService
{
    Task<List<PageDto>> GetBySiteAsync(Guid userId, Guid siteId);
    Task<PageDetailDto?> GetByIdAsync(Guid userId, Guid id);
    Task<PageDto?> CreateAsync(Guid userId, Guid siteId, CreatePageRequest request);
    Task<PageDto?> UpdateAsync(Guid userId, Guid id, UpdatePageRequest request);
    Task<bool> DeleteAsync(Guid userId, Guid id);
}
