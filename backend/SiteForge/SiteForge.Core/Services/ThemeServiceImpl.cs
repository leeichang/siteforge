using SiteForge.Core.DTOs;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;
using SiteForge.Core.Interfaces.Services;
using SiteForge.Core.Utilities;

namespace SiteForge.Core.Services;

public class ThemeServiceImpl : ThemeService
{
    private readonly RThemeRepository _themes;

    public ThemeServiceImpl(RThemeRepository themes)
    {
        _themes = themes;
    }

    public async Task<List<ThemeDto>> GetSystemAsync() =>
        (await _themes.GetSystemThemesAsync()).Select(Mappers.ToDto).ToList();

    public async Task<ThemeDto?> GetByIdAsync(Guid id)
    {
        var theme = await _themes.GetByIdAsync(id);
        return theme is null ? null : Mappers.ToDto(theme);
    }

    public async Task<ThemeDto> CreateAsync(CreateThemeRequest request)
    {
        var theme = await _themes.AddAsync(new Theme
        {
            Name = request.Name,
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            Colors = request.Colors,
            Fonts = request.Fonts,
            FontImportUrl = request.FontImportUrl,
            Spacing = request.Spacing,
            BorderRadius = request.BorderRadius,
            Shadows = request.Shadows,
            CustomCss = request.CustomCss,
            IsSystem = false
        });
        return Mappers.ToDto(theme);
    }

    public async Task<ThemeDto?> UpdateAsync(Guid id, UpdateThemeRequest request)
    {
        var theme = await _themes.GetByIdAsync(id);
        if (theme is null) return null;

        if (request.Name is not null) theme.Name = request.Name;
        if (request.Description is not null) theme.Description = request.Description;
        if (request.ThumbnailUrl is not null) theme.ThumbnailUrl = request.ThumbnailUrl;
        if (request.Colors is not null) theme.Colors = request.Colors;
        if (request.Fonts is not null) theme.Fonts = request.Fonts;
        if (request.FontImportUrl is not null) theme.FontImportUrl = request.FontImportUrl;
        if (request.Spacing is not null) theme.Spacing = request.Spacing;
        if (request.BorderRadius is not null) theme.BorderRadius = request.BorderRadius;
        if (request.Shadows is not null) theme.Shadows = request.Shadows;
        if (request.CustomCss is not null) theme.CustomCss = request.CustomCss;

        await _themes.UpdateAsync(theme);
        return Mappers.ToDto(theme);
    }

    public Task<bool> DeleteAsync(Guid id) => _themes.DeleteAsync(id);
}
