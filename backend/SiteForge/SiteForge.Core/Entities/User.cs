using SqlSugar;

namespace SiteForge.Core.Entities;

[SugarTable("users")]
public class User : BaseEntity
{
    /// <summary>
    /// 電子郵件（登入用）
    /// </summary>
    [SugarColumn(Length = 256, IsNullable = false)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 使用者名稱
    /// </summary>
    [SugarColumn(Length = 128)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 密碼雜湊
    /// </summary>
    [SugarColumn(Length = 512, IsNullable = false)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 顯示名稱
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = true)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// 頭像 URL
    /// </summary>
    [SugarColumn(Length = 1024, IsNullable = true)]
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// 角色 (admin, user)
    /// </summary>
    [SugarColumn(Length = 32, DefaultValue = "user")]
    public string Role { get; set; } = "user";

    /// <summary>
    /// 是否已驗證 Email
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// 最後登入時間
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Refresh Token（用於 JWT 續命）
    /// </summary>
    [SugarColumn(Length = 512, IsNullable = true)]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Refresh Token 過期時間
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? RefreshTokenExpiry { get; set; }

    // Navigation properties
    [Navigate(NavigateType.OneToMany, nameof(Site.UserId))]
    public List<Site> Sites { get; set; } = new();
}
