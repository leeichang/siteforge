using SiteForge.Core.Entities;

namespace SiteForge.Core.Interfaces.Repositories;

public interface RAiMessageRepository : IBaseRepository<AiMessage>
{
    Task<List<AiMessage>> GetByConversationIdAsync(Guid conversationId);
    Task<AiMessage?> GetLastAssistantMessageAsync(Guid conversationId);
}
