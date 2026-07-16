using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using SupportChat.Backend.Models.Requests;
using SupportChat.Backend.Models.Responses;
using SupportChat.Backend.Services.Interfaces;

namespace SupportChat.Backend.Endpoints;

public static class ChatEndpoints
{
    public static void MapChatEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/chats").WithTags("Chats").RequireAuthorization("Agent");

        group.MapGet("/", GetChatsForCompanyAsync)
            .WithName("GetChats")
            .WithDescription("Get all chats for the company.")
            .AllowAnonymous();

        group.MapGet("/{id}", GetChatByIdAsync)
            .WithName("GetChatById")
            .WithDescription("Get a specific chat with messages and details.");

        group.MapPost("/", CreateChatAsync)
            .WithName("CreateChat")
            .WithDescription("Create a new chat (initiated by customer or agent).")
            .AllowAnonymous();

        group.MapPost("/{id}/assign", AssignAgentAsync)
            .WithName("AssignAgent")
            .WithDescription("Assign an agent to a chat.");

        group.MapPost("/{id}/close", CloseChatAsync)
            .WithName("CloseChat")
            .WithDescription("Close a chat.");

        group.MapPost("/{id}/transfer", TransferChatAsync)
            .WithName("TransferChat")
            .WithDescription("Transfer chat to another agent.");
    }

    private static async Task<IResult> GetChatsForCompanyAsync(HttpContext httpContext, IChatService chatService, ILogger<Program> logger)
    {
        var companyId = httpContext.User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyId) || !long.TryParse(companyId, out var cid))
            return Results.BadRequest("Company not found.");

        var chats = await chatService.GetByCompanyAsync(cid);
        return Results.Ok(chats);
    }

    private static async Task<IResult> GetChatByIdAsync(long id, IChatService chatService, IMessageService messageService, HttpContext httpContext)
    {
        var chat = await chatService.GetByIdAsync(id);
        if (chat == null)
            return Results.NotFound($"Chat with ID {id} not found.");

        // Verify company ownership
        var companyId = httpContext.User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyId) || !long.TryParse(companyId, out var cid) || chat.CompanyId != cid)
            return Results.Forbid();

        var messages = await messageService.GetMessagesByChatAsync(id);
        return Results.Ok(new { Chat = chat, Messages = messages });
    }

    private static async Task<IResult> CreateChatAsync(CreateChatRequest request, IChatService chatService, ILogger<Program> logger)
    {
        try
        {
            var chatId = await chatService.CreateChatAsync(request);
            return Results.CreatedAtRoute("GetChatById", new { id = chatId }, new { ChatId = chatId });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating chat.");
            return Results.Problem("An error occurred while creating the chat.");
        }
    }

    private static async Task<IResult> AssignAgentAsync(long id, AssignChatRequest request, IChatService chatService, ILogger<Program> logger)
    {
        if (id != request.ChatId)
            return Results.BadRequest("ID mismatch.");

        try
        {
            await chatService.AssignAgentAsync(request.ChatId, request.AgentId);
            return Results.Ok(new { Message = "Agent assigned successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning agent.");
            return Results.Problem("An error occurred while assigning agent.");
        }
    }

    private static async Task<IResult> CloseChatAsync(HttpContext httpContext, long id, IChatService chatService, ILogger<Program> logger)
    {
        try
        {
            var agentId = httpContext.User.FindFirst("CompanyId")?.Value;

            await chatService.CloseChatAsync(id, Convert.ToInt64(agentId));
            return Results.Ok(new { Message = "Chat closed successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error closing chat.");
            return Results.Problem("An error occurred while closing chat.");
        }
    }

    private static async Task<IResult> TransferChatAsync(long id, TransferChatRequest request, IChatService chatService, ILogger<Program> logger)
    {
        if (id != request.ChatId)
            return Results.BadRequest("ID mismatch.");

        try
        {
            await chatService.TransferChatAsync(request.ChatId, request.NewAgentId);
            return Results.Ok(new { Message = "Chat transferred successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error transferring chat.");
            return Results.Problem("An error occurred while transferring chat.");
        }
    }
}