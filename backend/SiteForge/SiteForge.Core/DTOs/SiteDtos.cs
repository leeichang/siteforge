using SiteForge.Core.Entities;

namespace SiteForge.Core.DTOs;

public class CreateSiteRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? TemplateId { get; set; }
}

public class UpdateSiteRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? CustomHeaderScript { get; set; }
    public string? CustomFooterScript { get; set; }
}

public class SiteDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? FaviconUrl { get; set; }
    public string? Slug { get; set; }
    public string Status { get; set; } = "draft";
    public string? PublishedUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ThemeDto? ActiveTheme { get; set; }
    public List<SiteDomainDto> Domains { get; set; } = new();
    public List<LinkDto> Pages { get; set; } = new();
}

public class LinkDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsHomePage { get; set; }
    public int DisplayOrder { get; set; }
}

public class SiteDomainDto
{
    public Guid Id { get; set; }
    public string Domain { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsVerified { get; set; }
}

public class AddDomainRequest
{
    public string Domain { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}
