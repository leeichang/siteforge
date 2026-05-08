namespace SiteForge.Core.DTOs;

public class CreateConversationRequest
{
    public Guid SiteId { get; set; }
    public Guid? PageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4";
}

public class ConversationDto
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public Guid? PageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string Model { get; set; } = string.Empty;
    public int MessageCount { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime LastActivityAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SendMessageRequest
{
    public string Role { get; set; } = "user";
    public string Content { get; set; } = string.Empty;
    public string? ActionType { get; set; }
    public long? ClientTimestamp { get; set; }
}

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ActionType { get; set; }
    public string? ActionResult { get; set; }
    public string? Metadata { get; set; }
    public long ClientTimestamp { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AiGenerateSiteRequest
{
    public string SiteName { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Style { get; set; } = "studio";
    public string ContentLength { get; set; } = "medium";
    public List<string>? PageTypes { get; set; }
}

public class AiGeneratePageRequest
{
    public Guid SiteId { get; set; }
    public Guid? PageId { get; set; }
    public string PageName { get; set; } = string.Empty;
    public string PageType { get; set; } = "custom";
    public string Prompt { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string Style { get; set; } = "studio";
    public string ContentLength { get; set; } = "medium";
}

public class AiGeneratedPageDto
{
    public Guid PageId { get; set; }
    public Guid SiteId { get; set; }
    public string PageName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string PageType { get; set; } = string.Empty;
    public string HtmlContent { get; set; } = string.Empty;
    public string CssContent { get; set; } = string.Empty;
    public string? JsContent { get; set; }
    public string Components { get; set; } = "[]";
    public string Styles { get; set; } = "[]";
    public List<string> AiSuggestions { get; set; } = new();
    public long GenerationTimeMs { get; set; }
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
}

public class AiGenerateSiteResponse
{
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public List<AiGeneratedPageDto> Pages { get; set; } = new();
    public List<string> AiSuggestions { get; set; } = new();
    public long GenerationTimeMs { get; set; }
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
}

public class AiActionRequest
{
    public string Instruction { get; set; } = string.Empty;
    public string ActionType { get; set; } = "change_style";
    public Guid? PageId { get; set; }
    public Guid? WidgetId { get; set; }
    public string? Context { get; set; }
}

public class AiActionResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Result { get; set; }
    public List<string>? Suggestions { get; set; }
}

public class AssetDto
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string PublicUrl { get; set; } = string.Empty;
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? AltText { get; set; }
    public string Source { get; set; } = "upload";
    public DateTime CreatedAt { get; set; }
}

public class PublishTaskDto
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public int TotalPages { get; set; }
    public int PublishedPages { get; set; }
    public string? TargetUrl { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
