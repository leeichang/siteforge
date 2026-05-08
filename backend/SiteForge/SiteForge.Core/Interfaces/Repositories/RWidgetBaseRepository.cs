using SiteForge.Core.Entities;

namespace SiteForge.Core.Interfaces.Repositories;

public interface RWidgetBaseRepository : IBaseRepository<WidgetBase>
{
    Task<List<WidgetBase>> GetByPageIdAsync(Guid pageId);
    Task<List<WidgetBase>> GetByPageAndZoneAsync(Guid pageId, string zoneName);
    Task<bool> ReorderAsync(Guid widgetId, int newOrder);
}
