using SqlSugar;

namespace SiteForge.Core.Entities;

/// <summary>
/// 基礎實體 - 所有資料實體的基底類別
/// </summary>
public class BaseEntity
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 軟刪除標記
    /// </summary>
    public bool IsDeleted { get; set; }
}
