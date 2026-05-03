using SqlSugar;

namespace SiteForge.Core.Entities;

/// <summary>
/// 佈局模板 - 頁面結構（借鑒 ZKEACMS LayoutEntity）
/// </summary>
[SugarTable("layouts")]
public class Layout : BaseEntity
{
    /// <summary>
    /// 佈局名稱，如 "Full Width", "With Sidebar"
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 佈局描述
    /// </summary>
    [SugarColumn(Length = 512, IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 佈局預覽圖 URL
    /// </summary>
    [SugarColumn(Length = 1024, IsNullable = true)]
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// 佈局 HTML 骨架
    /// </summary>
    [SugarColumn(ColumnDataType = "text")]
    public string BodyHtml { get; set; } = string.Empty;

    /// <summary>
    /// 是否為系統內建
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties
    [Navigate(NavigateType.OneToMany, nameof(LayoutZone.LayoutId))]
    public List<LayoutZone> Zones { get; set; } = new();

    [Navigate(NavigateType.OneToMany, nameof(Page.LayoutId))]
    public List<Page> Pages { get; set; } = new();
}
