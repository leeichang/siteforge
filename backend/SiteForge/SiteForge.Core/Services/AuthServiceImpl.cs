using SiteForge.Core.DTOs;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;
using SiteForge.Core.Interfaces.Services;
using SiteForge.Core.Utilities;

namespace SiteForge.Core.Services;

public class AuthServiceImpl : AuthService
{
    private readonly RUserRepository _users;
    private readonly PasswordHelper _passwords;
    private readonly JwtHelper _jwt;

    public AuthServiceImpl(RUserRepository users, PasswordHelper passwords, JwtHelper jwt)
    {
        _users = users;
        _passwords = passwords;
        _jwt = jwt;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = NormalizeEmail(request.Email);
        if (await _users.EmailExistsAsync(email))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var user = await _users.AddAsync(new User
        {
            Email = email,
            Username = email,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? email : request.DisplayName.Trim(),
            PasswordHash = _passwords.HashPassword(request.Password),
            Role = "user",
            RefreshToken = _jwt.GenerateRefreshToken(),
            RefreshTokenExpiry = _jwt.GetRefreshTokenExpiry()
        });

        await _users.UpdateAsync(user);
        return ToAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _users.GetByEmailAsync(email);
        if (user is null || !_passwords.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid email or password.");
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.RefreshToken = _jwt.GenerateRefreshToken();
        user.RefreshTokenExpiry = _jwt.GetRefreshTokenExpiry();
        await _users.UpdateAsync(user);

        return ToAuthResponse(user);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request)
    {
        var users = await _users.GetAllAsync();
        var user = users.FirstOrDefault(x => x.RefreshToken == request.RefreshToken && x.RefreshTokenExpiry > DateTime.UtcNow);
        if (user is null)
        {
            throw new InvalidOperationException("Invalid refresh token.");
        }

        user.RefreshToken = _jwt.GenerateRefreshToken();
        user.RefreshTokenExpiry = _jwt.GetRefreshTokenExpiry();
        await _users.UpdateAsync(user);

        return ToAuthResponse(user);
    }

    public async Task<UserDto?> GetProfileAsync(Guid userId)
    {
        var user = await _users.GetByIdAsync(userId);
        return user is null ? null : Mappers.ToDto(user);
    }

    public async Task<UserDto?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _users.GetByIdAsync(userId);
        if (user is null) return null;

        if (request.DisplayName is not null) user.DisplayName = request.DisplayName.Trim();
        if (request.AvatarUrl is not null) user.AvatarUrl = request.AvatarUrl.Trim();
        await _users.UpdateAsync(user);
        return Mappers.ToDto(user);
    }

    private AuthResponse ToAuthResponse(User user) => new()
    {
        Token = _jwt.GenerateAccessToken(user),
        RefreshToken = user.RefreshToken ?? string.Empty,
        ExpiresAt = _jwt.GetAccessTokenExpiry(),
        User = Mappers.ToDto(user)
    };

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
