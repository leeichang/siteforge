using SqlSugar;

namespace SiteForge.Core.Entities;

/// <summary>
/// AI 對話訊息（參考 GrapesJS AiMessageEntity）
/// </summary>
[SugarTable("ai_messages")]
public class AiMessage : BaseEntity
{
    /// <summary>
    /// 所屬對話
    /// </summary>
    public Guid ConversationId { get; set; }

    /// <summary>
    /// 角色：user, assistant, system
    /// </summary>
    [SugarColumn(Length = 16, IsNullable = false)]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 訊息內容
    /// </summary>
    [SugarColumn(ColumnDataType = "text")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 動作類型（可選）：generate_section, generate_page, change_style, change_theme, rewrite, translate, generate_image
    /// </summary>
    [SugarColumn(Length = 32, IsNullable = true)]
    public string? ActionType { get; set; }

    /// <summary>
    /// 動作結果（JSON，包含生成的區塊資料等）
    /// </summary>
    [SugarColumn(ColumnDataType = "jsonb", IsNullable = true)]
    public string? ActionResult { get; set; }

    /// <summary>
    /// 擴展資訊（JSON，token用量、生成延遲等）
    /// </summary>
    [SugarColumn(ColumnDataType = "jsonb", IsNullable = true)]
    public string? Metadata { get; set; }

    /// <summary>
    /// 用戶端時間戳（用於排序）
    /// </summary>
    public long ClientTimestamp { get; set; }

    // Navigation properties
    [Navigate(NavigateType.OneToOne, nameof(ConversationId))]
    public AiConversation? Conversation { get; set; }
}
