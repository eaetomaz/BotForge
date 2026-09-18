using BotForge.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BotForge.Api.Controllers;

public record ChatRequestDto(string ConversationId, string Message);
public record ChatResponseDto(string Reply, bool Cleared);

[ApiController]
[Route("chat")]
public class ChatController(ChatService chatService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ChatResponseDto>> Ask(ChatRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ConversationId) || string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("ConversationId e Message são obrigatórios.");

        var result = await chatService.AskAsync(request.ConversationId, request.Message, ct);
        return Ok(new ChatResponseDto(result.Reply, result.Cleared));
    }

    [HttpDelete("{conversationId}")]
    public async Task<IActionResult> Clear(string conversationId, CancellationToken ct)
    {
        await chatService.ClearAsync(conversationId, ct);
        return NoContent();
    }
}
