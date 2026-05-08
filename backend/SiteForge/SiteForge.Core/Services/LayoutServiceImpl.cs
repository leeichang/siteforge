using SiteForge.Core.DTOs;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;
using SiteForge.Core.Interfaces.Services;
using SiteForge.Core.Utilities;

namespace SiteForge.Core.Services;

public class LayoutServiceImpl : LayoutService
{
    private readonly RLayoutRepository _layouts;
    private readonly RLayoutZoneRepository _zones;

    public LayoutServiceImpl(RLayoutRepository layouts, RLayoutZoneRepository zones)
    {
        _layouts = layouts;
        _zones = zones;
    }

    public async Task<List<LayoutDto>> GetActiveAsync()
    {
        var layouts = await _layouts.GetActiveLayoutsAsync();
        var result = new List<LayoutDto>();
        foreach (var layout in layouts)
        {
            result.Add(await ToLayoutDtoAsync(layout));
        }
        return result;
    }

    public async Task<LayoutDto?> GetByIdAsync(Guid id)
    {
        var layout = await _layouts.GetByIdAsync(id);
        return layout is null ? null : await ToLayoutDtoAsync(layout);
    }

    public async Task<LayoutDto> CreateAsync(CreateLayoutRequest request)
    {
        var layout = await _layouts.AddAsync(new Layout
        {
            Name = request.Name,
            Description = request.Description,
            BodyHtml = request.BodyHtml,
            IsSystem = false,
            IsActive = true
        });

        if (request.Zones.Count > 0)
        {
            await _zones.AddRangeAsync(request.Zones.Select(x => new LayoutZone
            {
                LayoutId = layout.Id,
                Name = x.Name,
                Title = x.Title,
                Width = x.Width,
                Order = x.Order,
                CssClass = x.CssClass,
                PlaceholderHtml = x.PlaceholderHtml,
                IsEditable = x.IsEditable
            }).ToList());
        }

        return await ToLayoutDtoAsync(layout);
    }

    public async Task<LayoutDto?> UpdateAsync(Guid id, UpdateLayoutRequest request)
    {
        var layout = await _layouts.GetByIdAsync(id);
        if (layout is null) return null;

        if (request.Name is not null) layout.Name = request.Name;
        if (request.Description is not null) layout.Description = request.Description;
        if (request.BodyHtml is not null) layout.BodyHtml = request.BodyHtml;
        await _layouts.UpdateAsync(layout);

        return await ToLayoutDtoAsync(layout);
    }

    public Task<bool> DeleteAsync(Guid id) => _layouts.DeleteAsync(id);

    private async Task<LayoutDto> ToLayoutDtoAsync(Layout layout)
    {
        var dto = Mappers.ToDto(layout);
        dto.Zones = (await _zones.GetByLayoutIdAsync(layout.Id)).Select(Mappers.ToDto).ToList();
        return dto;
    }
}
