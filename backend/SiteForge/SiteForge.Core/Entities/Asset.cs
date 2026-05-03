using SqlSugar;

namespace SiteForge.Core.Entities;

/// <summary>
/// 圖片資源 - 使用者上傳或 AI 生成
/// </summary>
[SugarTable("assets")]
public class Asset : BaseEntity
{
    /// <summary>
    /// 所屬網站
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// 檔案名稱
    /// </summary>
    [SugarColumn(Length = 256)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 檔案類型：image/png, image/jpeg, image/svg+xml
    /// </summary>
    [SugarColumn(Length = 64)]
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// 檔案大小（bytes）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 儲存路徑（或 S3/CDN URL）
    /// </summary>
    [SugarColumn(Length = 1024)]
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>
    /// 公開 URL
    /// </summary>
    [SugarColumn(Length = 1024)]
    public string PublicUrl { get; set; } = string.Empty;

    /// <summary>
    /// 寬度（圖片）
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// 高度（圖片）
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// 替代文字
    /// </summary>
    [SugarColumn(Length = 512, IsNullable = true)]
    public string? AltText { get; set; }

    /// <summary>
    /// 來源：upload, ai_generated, unsplash
    /// </summary>
    [SugarColumn(Length = 32, DefaultValue = "upload")]
    public string Source { get; set; } = "upload";

    /// <summary>
    /// 來源 URL（unsplash 原始連結等）
    /// </summary>
    [SugarColumn(Length = 1024, IsNullable = true)]
    public string? SourceUrl { get; set; }
}
