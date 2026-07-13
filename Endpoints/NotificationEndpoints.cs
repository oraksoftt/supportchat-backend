using Microsoft.AspNetCore.Http.HttpResults;
using SupportChat.Backend.Models.Requests;
using SupportChat.Backend.Services.Interfaces;
using System.Security.Claims;

namespace SupportChat.Backend.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/notifications")
            .WithTags("Notifications")
            .RequireAuthorization("Agent");

        group.MapGet("/", GetUserNotificationsAsync)
            .WithName("GetNotifications")
            .WithDescription("Get notifications for the current user.");

        group.MapPost("/{id}/read", MarkAsReadAsync)
            .WithName("MarkAsRead")
            .WithDescription("Mark a notification as read.");
    }

    private static async Task<IResult> GetUserNotificationsAsync(
        HttpContext httpContext,
        INotificationService notificationService)
    {
        var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        var notifications = await notificationService.GetUserNotificationsAsync(userId);
        return Results.Ok(notifications);
    }

    private static async Task<IResult> MarkAsReadAsync(
        long id,
        INotificationService notificationService,
        ILogger<Program> logger)
    {
        try
        {
            await notificationService.MarkAsReadAsync(id);
            return Results.Ok(new { Message = "Notification marked as read." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error marking notification as read.");
            return Results.Problem("An error occurred.");
        }
    }
}