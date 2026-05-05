namespace SiteForge.Core.DTOs;

public class CreatePageRequest
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public Guid? LayoutId { get; set; }
    public string PageType { get; set; } = "custom";
    public bool IsHome { get; set; }
}

public class UpdatePageRequest
{
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? LayoutId { get; set; }
    public string? PageType { get; set; }
    public bool? IsHome { get; set; }
    public string? Components { get; set; }
    public string? Styles { get; set; }
    public string? HtmlContent { get; set; }
    public string? CssContent { get; set; }
    public string? JsContent { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? ShowInNav { get; set; }
}

public class PageDto
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string PageType { get; set; } = string.Empty;
    public bool IsHome { get; set; }
    public bool IsPublished { get; set; }
    public int DisplayOrder { get; set; }
    public bool ShowInNav { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class PageDetailDto : PageDto
{
    public string? Components { get; set; }
    public string? Styles { get; set; }
    public string? HtmlContent { get; set; }
    public string? CssContent { get; set; }
    public string? JsContent { get; set; }
    public List<WidgetDto> Widgets { get; set; } = new();
    public List<PageDto>? Children { get; set; }
}

public class WidgetDto
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = "{}";
    public string? Style { get; set; }
    public string ZoneName { get; set; } = "main";
    public int Order { get; set; }
    public bool IsHidden { get; set; }
}

public class UpdateWidgetRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Style { get; set; }
    public string? ZoneName { get; set; }
    public int? Order { get; set; }
    public string? CustomCssClass { get; set; }
    public bool? IsHidden { get; set; }
}

public class AddWidgetRequest
{
    public Guid TemplateId { get; set; }
    public string ZoneName { get; set; } = "main";
    public int Order { get; set; }
    public string? Content { get; set; }
}
