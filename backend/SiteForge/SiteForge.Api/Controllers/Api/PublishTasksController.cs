using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteForge.Core.DTOs;
using SiteForge.Core.Interfaces.Services;

namespace SiteForge.Api.Controllers.Api;

[Authorize]
[ApiController]
[Area("api")]
[Route("[area]/[controller]")]
public class PublishTasksController : ApiControllerBase
{
    private readonly PublishTaskService _tasks;

    public PublishTasksController(PublishTaskService tasks)
    {
        _tasks = tasks;
    }

    [HttpGet("site/{siteId:guid}")]
    public async Task<ActionResult<ApiResponse<List<PublishTaskDto>>>> GetBySite(Guid siteId) =>
        OkResponse(await _tasks.GetBySiteAsync(siteId));

    [HttpGet("site/{siteId:guid}/latest")]
    public async Task<ActionResult<ApiResponse<PublishTaskDto>>> Latest(Guid siteId)
    {
        var task = await _tasks.GetLatestAsync(siteId);
        return task is null ? NotFoundResponse<PublishTaskDto>() : OkResponse(task);
    }

    [HttpPost("{id:guid}/retry")]
    public async Task<ActionResult<ApiResponse<PublishTaskDto>>> Retry(Guid id)
    {
        var task = await _tasks.RetryAsync(id);
        return task is null ? NotFoundResponse<PublishTaskDto>() : OkResponse(task, "Publish task queued for retry.");
    }
}
