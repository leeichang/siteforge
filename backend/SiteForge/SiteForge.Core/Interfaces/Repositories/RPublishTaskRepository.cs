using SiteForge.Core.Entities;

namespace SiteForge.Core.Interfaces.Repositories;

public interface RPublishTaskRepository : IBaseRepository<PublishTask>
{
    Task<List<PublishTask>> GetBySiteIdAsync(Guid siteId);
    Task<PublishTask?> GetLatestTaskAsync(Guid siteId);
    Task<List<PublishTask>> GetPendingTasksAsync();
}
