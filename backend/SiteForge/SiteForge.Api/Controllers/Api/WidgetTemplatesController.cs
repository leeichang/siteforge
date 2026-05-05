using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteForge.Core.DTOs;
using SiteForge.Core.Interfaces.Services;

namespace SiteForge.Api.Controllers.Api;

[AllowAnonymous]
[ApiController]
[Area("api")]
[Route("[area]/[controller]")]
public class WidgetTemplatesController : ApiControllerBase
{
    private readonly WidgetTemplateService _templates;

    public WidgetTemplatesController(WidgetTemplateService templates)
    {
        _templates = templates;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<WidgetTemplateDto>>>> GetAll([FromQuery] string? category) =>
        OkResponse(await _templates.GetAllAsync(category));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<WidgetTemplateDto>>> Get(Guid id)
    {
        var template = await _templates.GetByIdAsync(id);
        return template is null ? NotFoundResponse<WidgetTemplateDto>() : OkResponse(template);
    }
}
