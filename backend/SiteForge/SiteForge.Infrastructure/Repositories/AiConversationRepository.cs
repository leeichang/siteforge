using SqlSugar;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;

namespace SiteForge.Infrastructure.Repositories;

public class AiConversationRepository : BaseRepository<AiConversation>, RAiConversationRepository
{
    public AiConversationRepository(ISqlSugarClient db) : base(db) { }

    public Task<List<AiConversation>> GetBySiteIdAsync(Guid siteId) =>
        _db.Queryable<AiConversation>().Where(x => x.SiteId == siteId && !x.IsDeleted).OrderByDescending(x => x.LastActivityAt).ToListAsync();

    public Task<List<AiConversation>> GetByPageIdAsync(Guid pageId) =>
        _db.Queryable<AiConversation>().Where(x => x.PageId == pageId && !x.IsDeleted).OrderByDescending(x => x.LastActivityAt).ToListAsync();
}
