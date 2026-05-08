using SiteForge.Core.DTOs;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;
using SiteForge.Core.Interfaces.Services;
using SiteForge.Core.Utilities;

namespace SiteForge.Core.Services;

public class WidgetServiceImpl : WidgetService
{
    private readonly RWidgetBaseRepository _widgets;
    private readonly RWidgetTemplateRepository _templates;

    public WidgetServiceImpl(RWidgetBaseRepository widgets, RWidgetTemplateRepository templates)
    {
        _widgets = widgets;
        _templates = templates;
    }

    public async Task<List<WidgetDto>> GetByPageAsync(Guid pageId) =>
        (await _widgets.GetByPageIdAsync(pageId)).Select(Mappers.ToDto).ToList();

    public async Task<WidgetDto> AddAsync(Guid pageId, AddWidgetRequest request)
    {
        var template = await _templates.GetByIdAsync(request.TemplateId);
        var widget = await _widgets.AddAsync(new WidgetBase
        {
            PageId = pageId,
            TemplateId = request.TemplateId,
            Title = template?.Name ?? "Widget",
            Content = request.Content ?? template?.EditableProps ?? "{}",
            ZoneName = request.ZoneName,
            Order = request.Order
        });
        return Mappers.ToDto(widget);
    }

    public async Task<WidgetDto?> UpdateAsync(Guid id, UpdateWidgetRequest request)
    {
        var widget = await _widgets.GetByIdAsync(id);
        if (widget is null) return null;

        if (request.Title is not null) widget.Title = request.Title;
        if (request.Content is not null) widget.Content = request.Content;
        if (request.Style is not null) widget.Style = request.Style;
        if (request.ZoneName is not null) widget.ZoneName = request.ZoneName;
        if (request.Order.HasValue) widget.Order = request.Order.Value;
        if (request.CustomCssClass is not null) widget.CustomCssClass = request.CustomCssClass;
        if (request.IsHidden.HasValue) widget.IsHidden = request.IsHidden.Value;

        await _widgets.UpdateAsync(widget);
        return Mappers.ToDto(widget);
    }

    public Task<bool> DeleteAsync(Guid id) => _widgets.DeleteAsync(id);
}
