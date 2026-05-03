using SqlSugar;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;

namespace SiteForge.Infrastructure.Repositories;

/// <summary>
/// 基礎 Repository - 封裝 SqlSugar 的 CRUD 操作
/// </summary>
public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity, new()
{
    protected readonly ISqlSugarClient _db;

    public BaseRepository(ISqlSugarClient db)
    {
        _db = db;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        return await _db.Queryable<T>()
            .Where(x => x.Id == id && !x.IsDeleted)
            .FirstAsync();
    }

    public virtual async Task<List<T>> GetAllAsync(bool includeDeleted = false)
    {
        if (includeDeleted)
            return await _db.Queryable<T>().ToListAsync();
        
        return await _db.Queryable<T>()
            .Where(x => !x.IsDeleted)
            .ToListAsync();
    }

    public virtual async Task<List<T>> GetPagedAsync(int page, int pageSize, bool includeDeleted = false)
    {
        if (includeDeleted)
            return await _db.Queryable<T>()
                .OrderByDescending(x => x.CreatedAt)
                .ToPageListAsync(page, pageSize);
        
        return await _db.Queryable<T>()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToPageListAsync(page, pageSize);
    }

    public virtual async Task<int> GetCountAsync(bool includeDeleted = false)
    {
        if (includeDeleted)
            return await _db.Queryable<T>().CountAsync();
        
        return await _db.Queryable<T>()
            .Where(x => !x.IsDeleted)
            .CountAsync();
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        
        await _db.Insertable(entity).ExecuteReturnSnowflakeIdAsync();
        return entity;
    }

    public virtual async Task AddRangeAsync(List<T> entities)
    {
        foreach (var entity in entities)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
        }
        await _db.Insertable(entities).ExecuteReturnSnowflakeIdAsync();
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        return await _db.Updateable(entity)
            .IgnoreColumns(x => new { x.CreatedAt })
            .ExecuteCommandAsync() > 0;
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        // 軟刪除
        return await _db.Updateable<T>()
            .SetColumns(x => x.IsDeleted, true)
            .SetColumns(x => x.UpdatedAt, DateTime.UtcNow)
            .Where(x => x.Id == id)
            .ExecuteCommandAsync() > 0;
    }

    public virtual async Task<bool> HardDeleteAsync(Guid id)
    {
        return await _db.Deleteable<T>()
            .Where(x => x.Id == id)
            .ExecuteCommandAsync() > 0;
    }

    public virtual async Task<bool> RestoreAsync(Guid id)
    {
        return await _db.Updateable<T>()
            .SetColumns(x => x.IsDeleted, false)
            .SetColumns(x => x.UpdatedAt, DateTime.UtcNow)
            .Where(x => x.Id == id)
            .ExecuteCommandAsync() > 0;
    }
}
