namespace SiteForge.Core.DTOs;

public class WidgetTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? Description { get; set; }
    public string DefaultContent { get; set; } = string.Empty;
    public string? DefaultStyle { get; set; }
    public string? EditableProps { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateWidgetTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "Content";
    public string? ThumbnailUrl { get; set; }
    public string? Description { get; set; }
    public string DefaultContent { get; set; } = string.Empty;
    public string? DefaultStyle { get; set; }
    public string? EditableProps { get; set; }
}

public class UpdateWidgetTemplateRequest
{
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Description { get; set; }
    public string? DefaultContent { get; set; }
    public string? DefaultStyle { get; set; }
    public string? EditableProps { get; set; }
    public bool? IsActive { get; set; }
}
