namespace SiteForge.Core.DTOs;

public class ThemeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsSystem { get; set; }
    public string Colors { get; set; } = "{}";
    public string Fonts { get; set; } = "{}";
    public string? FontImportUrl { get; set; }
    public string? Spacing { get; set; }
    public string? BorderRadius { get; set; }
    public string? Shadows { get; set; }
    public string? CustomCss { get; set; }
    public string Version { get; set; } = "1.0";
    public DateTime CreatedAt { get; set; }
}

public class CreateThemeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string Colors { get; set; } = "{}";
    public string Fonts { get; set; } = "{}";
    public string? FontImportUrl { get; set; }
    public string? Spacing { get; set; }
    public string? BorderRadius { get; set; }
    public string? Shadows { get; set; }
    public string? CustomCss { get; set; }
}

public class UpdateThemeRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Colors { get; set; }
    public string? Fonts { get; set; }
    public string? FontImportUrl { get; set; }
    public string? Spacing { get; set; }
    public string? BorderRadius { get; set; }
    public string? Shadows { get; set; }
    public string? CustomCss { get; set; }
}
