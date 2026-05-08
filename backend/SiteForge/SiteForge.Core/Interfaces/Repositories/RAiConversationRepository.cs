using SiteForge.Core.Entities;

namespace SiteForge.Core.Interfaces.Repositories;

public interface RAiConversationRepository : IBaseRepository<AiConversation>
{
    Task<List<AiConversation>> GetBySiteIdAsync(Guid siteId);
    Task<List<AiConversation>> GetByPageIdAsync(Guid pageId);
}
