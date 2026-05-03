using SqlSugar;

namespace SiteForge.Core.Entities;

/// <summary>
/// AI 對話記錄（參考 GrapesJS AiConversationEntity）
/// </summary>
[SugarTable("ai_conversations")]
public class AiConversation : BaseEntity
{
    /// <summary>
    /// 所屬網站
    /// </summary>
    public Guid SiteId { get; set; }

    /// <summary>
    /// 關聯頁面（可為空，表示全域對話）
    /// </summary>
    public Guid? PageId { get; set; }

    /// <summary>
    /// 對話標題（自動摘要）
    /// </summary>
    [SugarColumn(Length = 256)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 對話摘要
    /// </summary>
    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string? Summary { get; set; }

    /// <summary>
    /// 使用的 AI 模型
    /// </summary>
    [SugarColumn(Length = 64)]
    public string Model { get; set; } = "gpt-4";

    /// <summary>
    /// Token 使用統計（JSON）
    /// </summary>
    [SugarColumn(ColumnDataType = "jsonb", IsNullable = true)]
    public string? TokenUsage { get; set; }

    /// <summary>
    /// 訊息數量
    /// </summary>
    public int MessageCount { get; set; }

    /// <summary>
    /// 是否已完成
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// 最後活躍時間
    /// </summary>
    public DateTime LastActivityAt { get; set; }

    // Navigation properties
    [Navigate(NavigateType.OneToMany, nameof(AiMessage.ConversationId))]
    public List<AiMessage> Messages { get; set; } = new();
}
