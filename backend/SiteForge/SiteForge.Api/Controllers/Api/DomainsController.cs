using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteForge.Core.DTOs;
using SiteForge.Core.Interfaces.Services;

namespace SiteForge.Api.Controllers.Api;

[Authorize]
[ApiController]
[Area("api")]
[Route("[area]/[controller]")]
public class DomainsController : ApiControllerBase
{
    private readonly DomainService _domains;

    public DomainsController(DomainService domains)
    {
        _domains = domains;
    }

    [HttpGet("site/{siteId:guid}")]
    public async Task<ActionResult<ApiResponse<List<SiteDomainDto>>>> GetBySite(Guid siteId) =>
        OkResponse(await _domains.GetBySiteAsync(siteId));

    [HttpPost("site/{siteId:guid}")]
    public async Task<ActionResult<ApiResponse<SiteDomainDto>>> Add(Guid siteId, AddDomainRequest request) =>
        OkResponse(await _domains.AddAsync(siteId, request), "Domain added.");

    [HttpPost("{id:guid}/verify")]
    public async Task<ActionResult<ApiResponse<SiteDomainDto>>> Verify(Guid id)
    {
        var domain = await _domains.VerifyAsync(id);
        return domain is null ? NotFoundResponse<SiteDomainDto>() : OkResponse(domain, "Domain verified.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id) =>
        OkResponse(await _domains.DeleteAsync(id), "Domain deleted.");
}
