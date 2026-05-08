using SiteForge.Core.Entities;

namespace SiteForge.Core.Interfaces.Repositories;

public interface RLayoutRepository : IBaseRepository<Layout>
{
    Task<Layout?> GetWithZonesAsync(Guid id);
    Task<List<Layout>> GetActiveLayoutsAsync();
}
