using SiteForge.Core.Entities;

namespace SiteForge.Core.Interfaces.Repositories;

public interface RWidgetTemplateRepository : IBaseRepository<WidgetTemplate>
{
    Task<List<WidgetTemplate>> GetByCategoryAsync(string category);
    Task<List<WidgetTemplate>> GetActiveTemplatesAsync();
}
