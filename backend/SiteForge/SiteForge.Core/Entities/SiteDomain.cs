using SqlSugar;

namespace SiteForge.Core.Entities;

/// <summary>
/// 網站域名綁定
/// </summary>
[SugarTable("site_domains")]
public class SiteDomain : BaseEntity
{
    /// <summary>
    /// 所屬網站
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// 完整域名，如 "example.com"
    /// </summary>
    [SugarColumn(Length = 256, IsNullable = false, IndexGroupName = "idx_domain")]
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// 是否為主域名
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// 是否已驗證所有權
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// 驗證 Token（TXT 記錄驗證）
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = true)]
    public string? VerificationToken { get; set; }

    /// <summary>
    /// SSL 是否啟用
    /// </summary>
    public bool SslEnabled { get; set; }

    /// <summary>
    /// SSL 憑證內容（加密儲存）
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string? SslCertificate { get; set; }

    /// <summary>
    /// SSL 憑證私鑰（加密儲存）
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string? SslPrivateKey { get; set; }

    /// <summary>
    /// SSL 到期日
    /// </summary>
    public DateTime? SslExpiry { get; set; }

    /// <summary>
    /// 是否為預設的 SiteForge 子網域
    /// </summary>
    public bool IsDefaultSubdomain { get; set; }

    /// <summary>
    /// DNS 配置狀態：pending, configured, error
    /// </summary>
    [SugarColumn(Length = 32, DefaultValue = "pending")]
    public string DnsStatus { get; set; } = "pending";
}
