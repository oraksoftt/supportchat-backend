using Microsoft.AspNetCore.Http.HttpResults;
using SupportChat.Backend.Models.Requests;
using SupportChat.Backend.Services.Interfaces;

namespace SupportChat.Backend.Endpoints;

public static class MessageEndpoints
{
    public static void MapMessageEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/messages").WithTags("Messages").AllowAnonymous(); 

        group.MapGet("/chat/{chatId}", GetMessagesForChatAsync)
            .WithName("GetMessages")
            .WithDescription("Get all messages for a chat.")
            .AllowAnonymous();

        group.MapPost("/", SendMessageAsync)
            .WithName("SendMessage")
            .WithDescription("Send a new message (via API, not SignalR).")
            .AllowAnonymous(); // Customers can send via API as fallback

        group.MapPost("/{id}/seen", MarkMessageAsSeenAsync)
            .WithName("MarkMessageSeen")
            .WithDescription("Mark a message as seen.");

        group.MapPost("/attachment", AddAttachmentAsync)
            .WithName("AddAttachment")
            .WithDescription("Add an attachment to a message.");
    }

    private static async Task<IResult> GetMessagesForChatAsync(long chatId, IMessageService messageService, IChatService chatService, HttpContext httpContext)
    {
        //var companyId = httpContext.User.FindFirst("CompanyId")?.Value;
        //if (string.IsNullOrEmpty(companyId) || !long.TryParse(companyId, out var cid))
        //    return Results.BadRequest("Company not found.");

        // Verify chat exists and belongs to caller's company
        var chat = await chatService.GetByIdAsync(chatId);
        if (chat == null)
            return Results.NotFound($"Chat with ID {chatId} not found.");

        //if (chat.CompanyId != cid)
        //    return Results.Forbid();

        var messages = await messageService.GetMessagesByChatAsync(chatId);
        return Results.Ok(messages);
    }

    private static async Task<IResult> SendMessageAsync(SendMessageRequest request,IMessageService messageService,ILogger<Program> logger)
    {
        try
        {
            var message = await messageService.SendMessageAsync(request);
            return Results.CreatedAtRoute("GetMessages", new { chatId = message.ChatId }, message);            
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending message.");
            return Results.Problem("An error occurred while sending the message.");
        }
    }

    private static async Task<IResult> MarkMessageAsSeenAsync(long id,IMessageService messageService,ILogger<Program> logger)
    {
        try
        {
            await messageService.MarkMessageAsSeenAsync(id);
            return Results.Ok(new { Message = "Message marked as seen." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error marking message as seen.");
            return Results.Problem("An error occurred.");
        }
    }

    private static async Task<IResult> AddAttachmentAsync(AddAttachmentRequest request,IMessageService messageService,ILogger<Program> logger)
    {
        try
        {
            var attachment = await messageService.AddAttachmentAsync(request);
            return Results.CreatedAtRoute("GetMessages", new { chatId = attachment.MessageId }, attachment);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding attachment.");
            return Results.Problem("An error occurred while adding attachment.");
        }
    }
}