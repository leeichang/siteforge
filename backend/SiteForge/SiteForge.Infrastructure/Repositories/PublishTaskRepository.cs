using SqlSugar;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;

namespace SiteForge.Infrastructure.Repositories;

public class PublishTaskRepository : BaseRepository<PublishTask>, RPublishTaskRepository
{
    public PublishTaskRepository(ISqlSugarClient db) : base(db) { }

    public Task<List<PublishTask>> GetBySiteIdAsync(Guid siteId) =>
        _db.Queryable<PublishTask>().Where(x => x.SiteId == siteId && !x.IsDeleted).OrderByDescending(x => x.CreatedAt).ToListAsync();

    public Task<PublishTask?> GetLatestTaskAsync(Guid siteId) =>
        _db.Queryable<PublishTask>().Where(x => x.SiteId == siteId && !x.IsDeleted).OrderByDescending(x => x.CreatedAt).FirstAsync();

    public Task<List<PublishTask>> GetPendingTasksAsync() =>
        _db.Queryable<PublishTask>().Where(x => x.Status == "pending" && !x.IsDeleted).OrderBy(x => x.CreatedAt).ToListAsync();
}
