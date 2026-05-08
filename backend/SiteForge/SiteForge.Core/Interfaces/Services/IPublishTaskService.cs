using SiteForge.Core.DTOs;

namespace SiteForge.Core.Interfaces.Services;

public interface PublishTaskService
{
    Task<List<PublishTaskDto>> GetBySiteAsync(Guid siteId);
    Task<PublishTaskDto?> GetLatestAsync(Guid siteId);
    Task<PublishTaskDto?> RetryAsync(Guid id);
}
