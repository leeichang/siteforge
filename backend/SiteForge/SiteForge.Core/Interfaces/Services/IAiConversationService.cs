using SiteForge.Core.DTOs;

namespace SiteForge.Core.Interfaces.Services;

public interface AiConversationService
{
    Task<List<ConversationDto>> GetBySiteAsync(Guid siteId);
    Task<ConversationDto> CreateAsync(CreateConversationRequest request);
    Task<List<MessageDto>> GetMessagesAsync(Guid conversationId);
    Task<MessageDto> SendMessageAsync(Guid conversationId, SendMessageRequest request);
    List<AiTemplateDto> GetTemplates(string? kind = null);
    Task<AiGenerateSiteResponse> GenerateSiteAsync(Guid userId, AiGenerateSiteRequest request);
    Task<AiGeneratedPageDto> GeneratePageAsync(Guid userId, AiGeneratePageRequest request);
}
