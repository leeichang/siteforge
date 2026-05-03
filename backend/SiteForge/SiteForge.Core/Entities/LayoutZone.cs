using SqlSugar;

namespace SiteForge.Core.Entities;

/// <summary>
/// 佈局區域 - 頁面中的區塊容器（借鑒 ZKEACMS LayoutHtmlEntity / LayoutZone）
/// </summary>
[SugarTable("layout_zones")]
public class LayoutZone : BaseEntity
{
    /// <summary>
    /// 所屬佈局
    /// </summary>
    public Guid LayoutId { get; set; }

    /// <summary>
    /// 區域名稱，如 "main", "sidebar", "header", "footer"
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 區域標題（顯示用）
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = true)]
    public string? Title { get; set; }

    /// <summary>
    /// 寬度比例（12-column grid）
    /// </summary>
    public int Width { get; set; } = 12;

    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 容器 CSS class
    /// </summary>
    [SugarColumn(Length = 256, IsNullable = true)]
    public string? CssClass { get; set; }

    /// <summary>
    /// 容器的區塊佔位符 HTML
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string? PlaceholderHtml { get; set; }

    /// <summary>
    /// 是否可被拖拽編輯
    /// </summary>
    public bool IsEditable { get; set; } = true;
}
