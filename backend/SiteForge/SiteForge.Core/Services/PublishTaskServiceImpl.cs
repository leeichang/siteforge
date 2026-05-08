using SiteForge.Core.DTOs;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;
using SiteForge.Core.Interfaces.Services;
using SiteForge.Core.Utilities;

namespace SiteForge.Core.Services;

public class PublishTaskServiceImpl : PublishTaskService
{
    private readonly RPublishTaskRepository _tasks;

    public PublishTaskServiceImpl(RPublishTaskRepository tasks)
    {
        _tasks = tasks;
    }

    public async Task<List<PublishTaskDto>> GetBySiteAsync(Guid siteId) =>
        (await _tasks.GetBySiteIdAsync(siteId)).Select(Mappers.ToDto).ToList();

    public async Task<PublishTaskDto?> GetLatestAsync(Guid siteId)
    {
        var task = await _tasks.GetLatestTaskAsync(siteId);
        return task is null ? null : Mappers.ToDto(task);
    }

    public async Task<PublishTaskDto?> RetryAsync(Guid id)
    {
        var task = await _tasks.GetByIdAsync(id);
        if (task is null) return null;

        task.Status = "pending";
        task.ErrorMessage = null;
        task.StartedAt = null;
        task.CompletedAt = null;
        await _tasks.UpdateAsync(task);
        return Mappers.ToDto(task);
    }
}
