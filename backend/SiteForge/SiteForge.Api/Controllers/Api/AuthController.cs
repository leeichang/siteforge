using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteForge.Core.DTOs;
using SiteForge.Core.Interfaces.Services;

namespace SiteForge.Api.Controllers.Api;

[AllowAnonymous]
[ApiController]
[Area("api")]
[Route("[area]/[controller]")]
public class AuthController : ApiControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(RegisterRequest request) =>
        OkResponse(await _auth.RegisterAsync(request), "Registered successfully.");

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request) =>
        OkResponse(await _auth.LoginAsync(request), "Logged in successfully.");

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh(RefreshTokenRequest request) =>
        OkResponse(await _auth.RefreshAsync(request), "Token refreshed.");
}
