using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteForge.Core.DTOs;
using SiteForge.Core.Interfaces.Services;

namespace SiteForge.Api.Controllers.Api;

[Authorize]
[ApiController]
[Area("api")]
[Route("[area]/[controller]")]
public class AiConversationsController : ApiControllerBase
{
    private readonly AiConversationService _conversations;

    public AiConversationsController(AiConversationService conversations)
    {
        _conversations = conversations;
    }

    [HttpGet("site/{siteId:guid}")]
    public async Task<ActionResult<ApiResponse<List<ConversationDto>>>> GetBySite(Guid siteId) =>
        OkResponse(await _conversations.GetBySiteAsync(siteId));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ConversationDto>>> Create(CreateConversationRequest request) =>
        OkResponse(await _conversations.CreateAsync(request), "Conversation created.");

    [HttpGet("{conversationId:guid}/messages")]
    public async Task<ActionResult<ApiResponse<List<MessageDto>>>> GetMessages(Guid conversationId) =>
        OkResponse(await _conversations.GetMessagesAsync(conversationId));

    [HttpPost("{conversationId:guid}/messages")]
    public async Task<ActionResult<ApiResponse<MessageDto>>> SendMessage(Guid conversationId, SendMessageRequest request) =>
        OkResponse(await _conversations.SendMessageAsync(conversationId, request), "Message saved.");
}
