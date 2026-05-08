using SqlSugar;

namespace SiteForge.Core.Entities;

/// <summary>
/// 區塊實例 - 頁面上的具體區塊（借鑒 ZKEACMS WidgetBase）
/// </summary>
[SugarTable("widget_bases")]
public class WidgetBase : BaseEntity
{
    /// <summary>
    /// 所屬頁面
    /// </summary>
    public Guid PageId { get; set; }

    /// <summary>
    /// 關聯的模板定義
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// 區塊標題（給編輯者辨識）
    /// </summary>
    [SugarColumn(Length = 256)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 區塊內容（JSON，包含標題、圖片、按鈕等數據）
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string? Content { get; set; }

    /// <summary>
    /// 區塊樣式覆蓋（JSON）
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string? Style { get; set; }

    /// <summary>
    /// 區塊渲染後的 HTML
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string? RenderedHtml { get; set; }

    /// <summary>
    /// 區塊渲染後的 CSS
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string? RenderedCss { get; set; }

    /// <summary>
    /// 區域名稱：main, sidebar, header, footer
    /// </summary>
    [SugarColumn(Length = 64, DefaultValue = "main")]
    public string ZoneName { get; set; } = "main";

    /// <summary>
    /// 排序
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 自訂 CSS Class
    /// </summary>
    [SugarColumn(Length = 256, IsNullable = true)]
    public string? CustomCssClass { get; set; }

    /// <summary>
    /// 自訂 ID
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = true)]
    public string? CustomId { get; set; }

    /// <summary>
    /// 是否隱藏
    /// </summary>
    public bool IsHidden { get; set; }

    // Navigation properties
    [Navigate(NavigateType.OneToOne, nameof(TemplateId))]
    public WidgetTemplate? Template { get; set; }
}
