using SqlSugar;

namespace SiteForge.Core.Entities;

/// <summary>
/// 區塊模板 - 定義區塊的骨架和可編輯區域（借鑒 ZKEACMS WidgetTemplate）
/// </summary>
[SugarTable("widget_templates")]
public class WidgetTemplate : BaseEntity
{
    /// <summary>
    /// 模板名稱，如 "Hero", "Features", "Team"
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 分類：Layout, Content, Media, Commerce
    /// </summary>
    [SugarColumn(Length = 64)]
    public string Category { get; set; } = "Content";

    /// <summary>
    /// 模板縮圖 URL
    /// </summary>
    [SugarColumn(Length = 1024, IsNullable = true)]
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// 模板描述
    /// </summary>
    [SugarColumn(Length = 512, IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 預設 HTML 內容（GrapesJS component 定義）
    /// </summary>
    [SugarColumn(ColumnDataType = "text")]
    public string DefaultContent { get; set; } = string.Empty;

    /// <summary>
    /// 預設 CSS 樣式
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string? DefaultStyle { get; set; }

    /// <summary>
    /// 可編輯參數定義（JSON，定義哪些屬性可編輯）
    /// </summary>
    [SugarColumn(ColumnDataType = "jsonb", IsNullable = true)]
    public string? EditableProps { get; set; }

    /// <summary>
    /// 是否為系統內建
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 顯示順序
    /// </summary>
    public int DisplayOrder { get; set; }

    // Navigation properties
    [Navigate(NavigateType.OneToMany, nameof(WidgetBase.TemplateId))]
    public List<WidgetBase>? Instances { get; set; }
}
