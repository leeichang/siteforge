using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteForge.Core.DTOs;
using SiteForge.Core.Interfaces.Services;

namespace SiteForge.Api.Controllers.Api;

[ApiController]
[Area("api")]
[Route("[area]/[controller]")]
public class LayoutsController : ApiControllerBase
{
    private readonly LayoutService _layouts;

    public LayoutsController(LayoutService layouts)
    {
        _layouts = layouts;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<LayoutDto>>>> GetActive() =>
        OkResponse(await _layouts.GetActiveAsync());

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<LayoutDto>>> Get(Guid id)
    {
        var layout = await _layouts.GetByIdAsync(id);
        return layout is null ? NotFoundResponse<LayoutDto>() : OkResponse(layout);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<LayoutDto>>> Create(CreateLayoutRequest request) =>
        OkResponse(await _layouts.CreateAsync(request), "Layout created.");

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<LayoutDto>>> Update(Guid id, UpdateLayoutRequest request)
    {
        var layout = await _layouts.UpdateAsync(id, request);
        return layout is null ? NotFoundResponse<LayoutDto>() : OkResponse(layout, "Layout updated.");
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id) =>
        OkResponse(await _layouts.DeleteAsync(id), "Layout deleted.");
}
