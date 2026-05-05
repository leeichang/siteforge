using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteForge.Core.DTOs;
using SiteForge.Core.Interfaces.Services;

namespace SiteForge.Api.Controllers.Api;

[Authorize]
[ApiController]
[Area("api")]
[Route("[area]/[controller]")]
public class AssetsController : ApiControllerBase
{
    private readonly AssetService _assets;

    public AssetsController(AssetService assets)
    {
        _assets = assets;
    }

    [HttpGet("site/{siteId:guid}")]
    public async Task<ActionResult<ApiResponse<List<AssetDto>>>> GetBySite(Guid siteId) =>
        OkResponse(await _assets.GetBySiteAsync(siteId));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AssetDto>>> Create(UploadAssetRequest request) =>
        OkResponse(await _assets.CreateAsync(request), "Asset registered.");
}
