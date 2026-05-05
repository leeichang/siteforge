using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteForge.Core.DTOs;
using SiteForge.Core.Interfaces.Services;

namespace SiteForge.Api.Controllers.Api;

[Authorize]
[ApiController]
[Area("api")]
[Route("[area]/[controller]")]
public class UsersController : ApiControllerBase
{
    private readonly AuthService _auth;

    public UsersController(AuthService auth)
    {
        _auth = auth;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Profile()
    {
        var profile = await _auth.GetProfileAsync(CurrentUserId);
        return profile is null ? NotFoundResponse<UserDto>() : OkResponse(profile);
    }

    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile(UpdateProfileRequest request)
    {
        var profile = await _auth.UpdateProfileAsync(CurrentUserId, request);
        return profile is null ? NotFoundResponse<UserDto>() : OkResponse(profile, "Profile updated.");
    }
}
