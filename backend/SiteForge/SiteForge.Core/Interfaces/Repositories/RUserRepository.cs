using SiteForge.Core.Entities;

namespace SiteForge.Core.Interfaces.Repositories;

public interface RUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
}
