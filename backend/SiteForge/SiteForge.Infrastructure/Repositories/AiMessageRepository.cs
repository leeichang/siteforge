using SqlSugar;
using SiteForge.Core.Entities;
using SiteForge.Core.Interfaces.Repositories;

namespace SiteForge.Infrastructure.Repositories;

public class AiMessageRepository : BaseRepository<AiMessage>, RAiMessageRepository
{
    public AiMessageRepository(ISqlSugarClient db) : base(db) { }

    public Task<List<AiMessage>> GetByConversationIdAsync(Guid conversationId) =>
        _db.Queryable<AiMessage>().Where(x => x.ConversationId == conversationId && !x.IsDeleted).OrderBy(x => x.CreatedAt).ToListAsync();

    public Task<AiMessage?> GetLastAssistantMessageAsync(Guid conversationId) =>
        _db.Queryable<AiMessage>().Where(x => x.ConversationId == conversationId && x.Role == "assistant" && !x.IsDeleted).OrderByDescending(x => x.CreatedAt).FirstAsync();
}
