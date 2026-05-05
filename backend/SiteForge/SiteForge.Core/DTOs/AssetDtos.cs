namespace SiteForge.Core.DTOs;

public class UploadAssetRequest
{
    public Guid SiteId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? AltText { get; set; }
    public string Source { get; set; } = "upload";
}
