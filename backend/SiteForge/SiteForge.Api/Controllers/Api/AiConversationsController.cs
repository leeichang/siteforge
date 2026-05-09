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

    [HttpGet("templates")]
    public ActionResult<ApiResponse<List<AiTemplateDto>>> GetTemplates([FromQuery] string? kind = null) =>
        OkResponse(_conversations.GetTemplates(kind));

    [HttpPost("generate-site")]
    public async Task<ActionResult<ApiResponse<AiGenerateSiteResponse>>> GenerateSite(AiGenerateSiteRequest request) =>
        OkResponse(await _conversations.GenerateSiteAsync(CurrentUserId, request), "Website generated.");

    [HttpPost("generate-page")]
    public async Task<ActionResult<ApiResponse<AiGeneratedPageDto>>> GeneratePage(AiGeneratePageRequest request) =>
        OkResponse(await _conversations.GeneratePageAsync(CurrentUserId, request), "Page generated.");

    [HttpGet("page-types")]
    public ActionResult<ApiResponse<List<object>>> GetPageTypes()
    {
        var pageTypes = new List<object>
        {
            new { value = "home", label = "Home", description = "Homepage with hero, features, proof, and CTA" },
            new { value = "about", label = "About", description = "Brand story, positioning, mission, and team narrative" },
            new { value = "services", label = "Services", description = "Service cards and value proposition sections" },
            new { value = "product", label = "Products", description = "Product showcase and benefits grid" },
            new { value = "portfolio", label = "Portfolio", description = "Case studies and work highlights" },
            new { value = "blog", label = "Blog", description = "Editorial landing page and article cards" },
            new { value = "contact", label = "Contact", description = "Contact form and next-step information" }
        };

        return OkResponse(pageTypes);
    }
}
