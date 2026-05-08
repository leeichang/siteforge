using SqlSugar;

namespace SiteForge.Core.Entities;

[SugarTable("pages")]
public class Page : BaseEntity
{
    /// <summary>
    /// 所屬網站
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// 父頁面（階層式導航）
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 頁面佈局
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? LayoutId { get; set; }

    /// <summary>
    /// 頁面標題
    /// </summary>
    [SugarColumn(Length = 256, IsNullable = false)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// URL Slug（唯一）
    /// </summary>
    [SugarColumn(Length = 256, IsNullable = false)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// 頁面類型：home, about, product, blog, contact, custom
    /// </summary>
    [SugarColumn(Length = 32, DefaultValue = "custom")]
    public string PageType { get; set; } = "custom";

    /// <summary>
    /// 是否為首頁
    /// </summary>
    public bool IsHome { get; set; }

    /// <summary>
    /// 渲染後的完整 HTML（從 GrapesJS 組件編譯）
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string? HtmlContent { get; set; }

    /// <summary>
    /// 編譯後的 CSS
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string? CssContent { get; set; }

    /// <summary>
    /// 頁面 JavaScript
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string? JsContent { get; set; }

    /// <summary>
    /// GrapesJS 組件結構（JSON）
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string Components { get; set; } = "[]";

    /// <summary>
    /// GrapesJS 樣式定義（JSON）
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string Styles { get; set; } = "[]";

    // SEO
    [SugarColumn(Length = 256, IsNullable = true)]
    public string? MetaTitle { get; set; }

    [SugarColumn(Length = 512, IsNullable = true)]
    public string? MetaDescription { get; set; }

    [SugarColumn(Length = 512, IsNullable = true)]
    public string? MetaKeywords { get; set; }

    [SugarColumn(Length = 1024, IsNullable = true)]
    public string? OgImageUrl { get; set; }

    // Status
    public bool IsPublished { get; set; }

    public int DisplayOrder { get; set; }

    /// <summary>
    /// 導航是否顯示
    /// </summary>
    public bool ShowInNav { get; set; } = true;

    [SugarColumn(IsNullable = true)]
    public DateTime? PublishedAt { get; set; }

    // Navigation properties
    [Navigate(NavigateType.OneToMany, nameof(Page.ParentId))]
    public List<Page>? Children { get; set; }

    [Navigate(NavigateType.OneToMany, nameof(WidgetBase.PageId))]
    public List<WidgetBase>? Widgets { get; set; }

    [Navigate(NavigateType.OneToMany, nameof(AiConversation.PageId))]
    public List<AiConversation>? Conversations { get; set; }
}
