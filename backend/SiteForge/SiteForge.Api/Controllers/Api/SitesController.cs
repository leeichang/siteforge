using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteForge.Core.DTOs;
using SiteForge.Core.Interfaces.Services;

namespace SiteForge.Api.Controllers.Api;

[Authorize]
[ApiController]
[Area("api")]
[Route("[area]/[controller]")]
public class SitesController : ApiControllerBase
{
    private readonly SiteService _sites;

    public SitesController(SiteService sites)
    {
        _sites = sites;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SiteDto>>>> GetMySites() =>
        OkResponse(await _sites.GetByUserAsync(CurrentUserId));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SiteDto>>> Get(Guid id)
    {
        var site = await _sites.GetByIdAsync(CurrentUserId, id);
        return site is null ? NotFoundResponse<SiteDto>() : OkResponse(site);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SiteDto>>> Create(CreateSiteRequest request) =>
        OkResponse(await _sites.CreateAsync(CurrentUserId, request), "Site created.");

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SiteDto>>> Update(Guid id, UpdateSiteRequest request)
    {
        var site = await _sites.UpdateAsync(CurrentUserId, id, request);
        return site is null ? NotFoundResponse<SiteDto>() : OkResponse(site, "Site updated.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var deleted = await _sites.DeleteAsync(CurrentUserId, id);
        return deleted ? OkResponse(true, "Site deleted.") : NotFoundResponse<bool>();
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<ApiResponse<PublishTaskDto>>> Publish(Guid id, PublishRequest request)
    {
        var task = await _sites.PublishAsync(CurrentUserId, id, request);
        return task is null ? NotFoundResponse<PublishTaskDto>() : OkResponse(task, "Publish task queued.");
    }
}
