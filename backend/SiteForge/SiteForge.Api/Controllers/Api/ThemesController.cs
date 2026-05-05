using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteForge.Core.DTOs;
using SiteForge.Core.Interfaces.Services;

namespace SiteForge.Api.Controllers.Api;

[ApiController]
[Area("api")]
[Route("[area]/[controller]")]
public class ThemesController : ApiControllerBase
{
    private readonly ThemeService _themes;

    public ThemesController(ThemeService themes)
    {
        _themes = themes;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ThemeDto>>>> GetSystem() =>
        OkResponse(await _themes.GetSystemAsync());

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ThemeDto>>> Get(Guid id)
    {
        var theme = await _themes.GetByIdAsync(id);
        return theme is null ? NotFoundResponse<ThemeDto>() : OkResponse(theme);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ThemeDto>>> Create(CreateThemeRequest request) =>
        OkResponse(await _themes.CreateAsync(request), "Theme created.");

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ThemeDto>>> Update(Guid id, UpdateThemeRequest request)
    {
        var theme = await _themes.UpdateAsync(id, request);
        return theme is null ? NotFoundResponse<ThemeDto>() : OkResponse(theme, "Theme updated.");
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id) =>
        OkResponse(await _themes.DeleteAsync(id), "Theme deleted.");
}
