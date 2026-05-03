using SqlSugar;

namespace SiteForge.Core.Entities;

/// <summary>
/// 發佈任務佇列（借鑒 ZKEACMS PublishService）
/// </summary>
[SugarTable("publish_tasks")]
public class PublishTask : BaseEntity
{
    /// <summary>
    /// 所屬網站
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// 任務類型：full_publish, incremental, rollback
    /// </summary>
    [SugarColumn(Length = 32, DefaultValue = "full_publish")]
    public string TaskType { get; set; } = "full_publish";

    /// <summary>
    /// 狀態：pending, publishing, done, failed
    /// </summary>
    [SugarColumn(Length = 32, DefaultValue = "pending")]
    public string Status { get; set; } = "pending";

    /// <summary>
    /// 錯誤訊息
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 發佈日誌
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string? Log { get; set; }

    /// <summary>
    /// 發佈總頁數
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 成功發佈頁數
    /// </summary>
    public int PublishedPages { get; set; }

    /// <summary>
    /// 發佈目標 URL
    /// </summary>
    [SugarColumn(Length = 512, IsNullable = true)]
    public string? TargetUrl { get; set; }

    /// <summary>
    /// 開始時間
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// 完成時間
    /// </summary>
    public DateTime? CompletedAt { get; set; }
}
