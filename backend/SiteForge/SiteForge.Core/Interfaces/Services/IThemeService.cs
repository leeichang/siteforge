using SiteForge.Core.DTOs;

namespace SiteForge.Core.Interfaces.Services;

public interface ThemeService
{
    Task<List<ThemeDto>> GetSystemAsync();
    Task<ThemeDto?> GetByIdAsync(Guid id);
    Task<ThemeDto> CreateAsync(CreateThemeRequest request);
    Task<ThemeDto?> UpdateAsync(Guid id, UpdateThemeRequest request);
    Task<bool> DeleteAsync(Guid id);
}
