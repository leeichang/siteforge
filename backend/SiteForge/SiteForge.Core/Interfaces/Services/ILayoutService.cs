using SiteForge.Core.DTOs;

namespace SiteForge.Core.Interfaces.Services;

public interface LayoutService
{
    Task<List<LayoutDto>> GetActiveAsync();
    Task<LayoutDto?> GetByIdAsync(Guid id);
    Task<LayoutDto> CreateAsync(CreateLayoutRequest request);
    Task<LayoutDto?> UpdateAsync(Guid id, UpdateLayoutRequest request);
    Task<bool> DeleteAsync(Guid id);
}
