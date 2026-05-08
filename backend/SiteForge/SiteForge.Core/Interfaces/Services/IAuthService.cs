using SiteForge.Core.DTOs;

namespace SiteForge.Core.Interfaces.Services;

public interface AuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshAsync(RefreshTokenRequest request);
    Task<UserDto?> GetProfileAsync(Guid userId);
    Task<UserDto?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
}
