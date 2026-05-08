using SqlSugar;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;

namespace SiteForge.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, RUserRepository
{
    public UserRepository(ISqlSugarClient db) : base(db) { }

    public Task<User?> GetByEmailAsync(string email) =>
        _db.Queryable<User>().Where(x => x.Email == email && !x.IsDeleted).FirstAsync();

    public Task<bool> EmailExistsAsync(string email) =>
        _db.Queryable<User>().AnyAsync(x => x.Email == email && !x.IsDeleted);
}
