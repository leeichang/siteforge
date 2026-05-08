using SiteForge.Core.Entities;

namespace SiteForge.Core.Interfaces.Repositories;

public interface IBaseRepository<T> where T : BaseEntity, new()
{
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync(bool includeDeleted = false);
    Task<List<T>> GetPagedAsync(int page, int pageSize, bool includeDeleted = false);
    Task<int> GetCountAsync(bool includeDeleted = false);
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(List<T> entities);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> HardDeleteAsync(Guid id);
    Task<bool> RestoreAsync(Guid id);
}
