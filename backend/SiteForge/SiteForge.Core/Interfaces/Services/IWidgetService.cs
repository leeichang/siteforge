using SiteForge.Core.DTOs;

namespace SiteForge.Core.Interfaces.Services;

public interface WidgetService
{
    Task<List<WidgetDto>> GetByPageAsync(Guid pageId);
    Task<WidgetDto> AddAsync(Guid pageId, AddWidgetRequest request);
    Task<WidgetDto?> UpdateAsync(Guid id, UpdateWidgetRequest request);
    Task<bool> DeleteAsync(Guid id);
}
