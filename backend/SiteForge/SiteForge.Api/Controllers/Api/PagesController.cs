using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteForge.Core.DTOs;
using SiteForge.Core.Interfaces.Services;

namespace SiteForge.Api.Controllers.Api;

[Authorize]
[ApiController]
[Area("api")]
[Route("[area]/[controller]")]
public class PagesController : ApiControllerBase
{
    private readonly PageService _pages;

    public PagesController(PageService pages)
    {
        _pages = pages;
    }

    [HttpGet("site/{siteId:guid}")]
    public async Task<ActionResult<ApiResponse<List<PageDto>>>> GetBySite(Guid siteId) =>
        OkResponse(await _pages.GetBySiteAsync(CurrentUserId, siteId));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PageDetailDto>>> Get(Guid id)
    {
        var page = await _pages.GetByIdAsync(CurrentUserId, id);
        return page is null ? NotFoundResponse<PageDetailDto>() : OkResponse(page);
    }

    [HttpPost("site/{siteId:guid}")]
    public async Task<ActionResult<ApiResponse<PageDto>>> Create(Guid siteId, CreatePageRequest request)
    {
        var page = await _pages.CreateAsync(CurrentUserId, siteId, request);
        return page is null ? NotFoundResponse<PageDto>() : OkResponse(page, "Page created.");
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PageDto>>> Update(Guid id, UpdatePageRequest request)
    {
        var page = await _pages.UpdateAsync(CurrentUserId, id, request);
        return page is null ? NotFoundResponse<PageDto>() : OkResponse(page, "Page updated.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id) =>
        OkResponse(await _pages.DeleteAsync(CurrentUserId, id), "Page deleted.");
}
