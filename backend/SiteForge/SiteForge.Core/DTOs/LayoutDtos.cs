namespace SiteForge.Core.DTOs;

public class LayoutDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string BodyHtml { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; }
    public List<LayoutZoneDto> Zones { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class LayoutZoneDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int Width { get; set; }
    public int Order { get; set; }
    public string? CssClass { get; set; }
    public string? PlaceholderHtml { get; set; }
    public bool IsEditable { get; set; }
}

public class CreateLayoutRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string BodyHtml { get; set; } = string.Empty;
    public List<CreateZoneRequest> Zones { get; set; } = new();
}

public class CreateZoneRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int Width { get; set; } = 12;
    public int Order { get; set; }
    public string? CssClass { get; set; }
    public string? PlaceholderHtml { get; set; }
    public bool IsEditable { get; set; } = true;
}

public class UpdateLayoutRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? BodyHtml { get; set; }
}
