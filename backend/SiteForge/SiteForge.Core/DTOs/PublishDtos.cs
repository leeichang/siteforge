namespace SiteForge.Core.DTOs;

public class PublishRequest
{
    public string TaskType { get; set; } = "full_publish";
    public string? TargetUrl { get; set; }
}
