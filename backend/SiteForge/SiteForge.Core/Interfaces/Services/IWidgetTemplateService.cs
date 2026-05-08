using SiteForge.Core.DTOs;

namespace SiteForge.Core.Interfaces.Services;

public interface WidgetTemplateService
{
    Task<List<WidgetTemplateDto>> GetAllAsync(string? category = null);
    Task<WidgetTemplateDto?> GetByIdAsync(Guid id);
}
