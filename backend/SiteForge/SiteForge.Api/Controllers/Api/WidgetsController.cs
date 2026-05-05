using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteForge.Core.DTOs;
using SiteForge.Core.Interfaces.Services;

namespace SiteForge.Api.Controllers.Api;

[Authorize]
[ApiController]
[Area("api")]
[Route("[area]/[controller]")]
public class WidgetsController : ApiControllerBase
{
    private readonly WidgetService _widgets;

    public WidgetsController(WidgetService widgets)
    {
        _widgets = widgets;
    }

    [HttpGet("page/{pageId:guid}")]
    public async Task<ActionResult<ApiResponse<List<WidgetDto>>>> GetByPage(Guid pageId) =>
        OkResponse(await _widgets.GetByPageAsync(pageId));

    [HttpPost("page/{pageId:guid}")]
    public async Task<ActionResult<ApiResponse<WidgetDto>>> Add(Guid pageId, AddWidgetRequest request) =>
        OkResponse(await _widgets.AddAsync(pageId, request), "Widget added.");

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<WidgetDto>>> Update(Guid id, UpdateWidgetRequest request)
    {
        var widget = await _widgets.UpdateAsync(id, request);
        return widget is null ? NotFoundResponse<WidgetDto>() : OkResponse(widget, "Widget updated.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id) =>
        OkResponse(await _widgets.DeleteAsync(id), "Widget deleted.");
}
