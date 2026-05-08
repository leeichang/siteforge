using SqlSugar;

namespace SiteForge.Core.Entities;

[SugarTable("sites")]
public class Site : BaseEntity
{
    /// <summary>
    /// 網站名稱
    /// </summary>
    [SugarColumn(Length = 256, IsNullable = false)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 網站描述
    /// </summary>
    [SugarColumn(Length = 1024, IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 網站 Logo URL
    /// </summary>
    [SugarColumn(Length = 1024, IsNullable = true)]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Favicon URL
    /// </summary>
    [SugarColumn(Length = 1024, IsNullable = true)]
    public string? FaviconUrl { get; set; }

    /// <summary>
    /// 所屬使用者
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 啟用中的主題
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public Guid? ActiveThemeId { get; set; }

    /// <summary>
    /// 網站狀態 (draft, published, suspended)
    /// </summary>
    [SugarColumn(Length = 32, DefaultValue = "draft")]
    public string Status { get; set; } = "draft";

    /// <summary>
    /// 網站 Slug（用於子網域）
    /// </summary>
    [SugarColumn(Length = 128, IsNullable = true)]
    public string? Slug { get; set; }

    /// <summary>
    /// Google Analytics ID
    /// </summary>
    [SugarColumn(Length = 64, IsNullable = true)]
    public string? GoogleAnalyticsId { get; set; }

    /// <summary>
    /// 自訂 Header Script
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string? CustomHeaderScript { get; set; }

    /// <summary>
    /// 自訂 Footer Script
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string? CustomFooterScript { get; set; }

    /// <summary>
    /// 網站發佈後的公開 URL
    /// </summary>
    [SugarColumn(Length = 512, IsNullable = true)]
    public string? PublishedUrl { get; set; }

    /// <summary>
    /// 最後發佈時間
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? PublishedAt { get; set; }

    // Navigation properties
    [Navigate(NavigateType.OneToMany, nameof(Page.SiteId))]
    public List<Page> Pages { get; set; } = new();

    [Navigate(NavigateType.OneToMany, nameof(SiteDomain.SiteId))]
    public List<SiteDomain> Domains { get; set; } = new();

    [Navigate(NavigateType.OneToMany, nameof(AiConversation.SiteId))]
    public List<AiConversation> Conversations { get; set; } = new();

    [Navigate(NavigateType.OneToOne, nameof(ActiveThemeId))]
    public Theme? ActiveTheme { get; set; }

    [Navigate(NavigateType.OneToMany, nameof(PublishTask.SiteId))]
    public List<PublishTask> PublishTasks { get; set; } = new();
}
