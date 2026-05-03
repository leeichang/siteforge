using SqlSugar;

namespace SiteForge.Core.Entities;

/// <summary>
/// 主題 - 全域視覺風格配置（借鑒 ZKEACMS ThemeEntity）
/// </summary>
[SugarTable("themes")]
public class Theme : BaseEntity
{
    /// <summary>
    /// 主題名稱
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 主題描述
    /// </summary>
    [SugarColumn(Length = 512, IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 主題縮圖 URL
    /// </summary>
    [SugarColumn(Length = 1024, IsNullable = true)]
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// 是否為系統內建主題
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// 顏色配置 JSON：{ primary, secondary, accent, background, text, success, warning, danger }
    /// </summary>
    [SugarColumn(ColumnDataType = "jsonb")]
    public string Colors { get; set; } = "{}";

    /// <summary>
    /// 字型配置 JSON：{ heading: "Inter", body: "Inter" }
    /// </summary>
    [SugarColumn(ColumnDataType = "jsonb")]
    public string Fonts { get; set; } = "{}";

    /// <summary>
    /// Google Fonts 匯入 URL
    /// </summary>
    [SugarColumn(Length = 1024, IsNullable = true)]
    public string? FontImportUrl { get; set; }

    /// <summary>
    /// 間距配置 JSON：{ unit: "px", baseSize: 16 }
    /// </summary>
    [SugarColumn(ColumnDataType = "jsonb", IsNullable = true)]
    public string? Spacing { get; set; }

    /// <summary>
    /// 圓角配置 JSON：{ sm: "4px", md: "8px", lg: "16px", full: "9999px" }
    /// </summary>
    [SugarColumn(ColumnDataType = "jsonb", IsNullable = true)]
    public string? BorderRadius { get; set; }

    /// <summary>
    /// 陰影配置 JSON：{ sm, md, lg }
    /// </summary>
    [SugarColumn(ColumnDataType = "jsonb", IsNullable = true)]
    public string? Shadows { get; set; }

    /// <summary>
    /// 自訂全局 CSS
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string? CustomCss { get; set; }

    /// <summary>
    /// 主題版本
    /// </summary>
    [SugarColumn(Length = 16, DefaultValue = "1.0")]
    public string Version { get; set; } = "1.0";
}
