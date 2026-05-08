using SiteForge.Core.DTOs;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;
using SiteForge.Core.Interfaces.Services;
using SiteForge.Core.Utilities;

namespace SiteForge.Core.Services;

public class WidgetTemplateServiceImpl : WidgetTemplateService
{
    private readonly RWidgetTemplateRepository _templates;

    public WidgetTemplateServiceImpl(RWidgetTemplateRepository templates)
    {
        _templates = templates;
    }

    public async Task<List<WidgetTemplateDto>> GetAllAsync(string? category = null)
    {
        var templates = string.IsNullOrWhiteSpace(category)
            ? await _templates.GetActiveTemplatesAsync()
            : await _templates.GetByCategoryAsync(category);
        return templates.Select(Mappers.ToDto).ToList();
    }

    public async Task<WidgetTemplateDto?> GetByIdAsync(Guid id)
    {
        var template = await _templates.GetByIdAsync(id);
        return template is null ? null : Mappers.ToDto(template);
    }
}
