using System.Text.Json;
using SupportChat.Backend.Constants;
using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Models.Requests;
using SupportChat.Backend.Repositories.Interfaces;
using SupportChat.Backend.Services.Interfaces;

namespace SupportChat.Backend.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(INotificationRepository notificationRepo, IEmailService emailService, ILogger<NotificationService> logger)
    {
        _notificationRepo = notificationRepo;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<long> CreateNotificationAsync(CreateNotificationRequest request)
    {
        var notification = new Notification
        {
            CompanyId = request.CompanyId,
            UserId = request.UserId,
            CustomerId = request.CustomerId,
            Type = request.NotificationType,
            Title = request.Title ?? string.Empty,
            Message = request.Message ?? string.Empty,
            CreatedOn = DateTime.UtcNow
        };

        // Interface requires companyId, notification, value, newChat flag (byte).
        // We don't have an extra value here, pass null and newChat = 0.
        return await _notificationRepo.CreateNotificationAsync(request.CompanyId, notification, null, 0);
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(long userId)
    {
        return await _notificationRepo.GetNotificationsByUserAsync(userId);
    }

    public async Task MarkAsReadAsync(long notificationId)
    {
        await _notificationRepo.MarkNotificationAsReadAsync(notificationId);
    }

    public async Task NotifyNewChatAsync(long companyId, long chatId, long customerId)
    {
        var title = "New Chat Started";
        var message = $"A new chat (ID: {chatId}) has been started and is waiting for assignment.";

        var notification = new Notification
        {
            CompanyId = companyId,
            UserId = null,
            CustomerId = customerId,
            Type = NotificationType.NewChat,
            Title = title,
            Message = message,
            CreatedOn = DateTime.UtcNow
        };

        // Create notification (newChat flag = 1 to indicate new-chat broadcast; adjust if semantics differ)
        await _notificationRepo.CreateNotificationAsync(companyId, notification, null, 1);

        // Queue system notification for downstream processing (emails, push, etc.)
        await QueueSystemNotificationAsync(companyId, (byte)NotificationType.NewChat, JsonSerializer.Serialize(new { ChatId = chatId, Message = message }));
    }

    public async Task NotifyChatAssignedAsync(long chatId, long agentId)
    {
        var title = "Chat Assigned";
        var message = $"Chat (ID: {chatId}) has been assigned to you.";

        var notification = new Notification
        {
            CompanyId = 0, // TODO: if companyId is required, obtain it from caller or repository
            UserId = agentId,
            CustomerId = null,
            Type = NotificationType.ChatAssigned,
            Title = title,
            Message = message,
            CreatedOn = DateTime.UtcNow
        };

        // We don't have companyId here; by design repository signature requires it.
        // Use 0 as placeholder — caller should pass companyId if available.
        await _notificationRepo.CreateNotificationAsync(0, notification, null, 0);
    }

    public async Task NotifyAgentOfflineAsync(long companyId, long chatId)
    {
        var title = "Agent Offline";
        var message = $"A customer is waiting, but no agents are online. Chat ID: {chatId}";

        _logger.LogInformation("NotifyAgentOfflineAsync companyId={CompanyId} chatId={ChatId}", companyId, chatId);

        // Queue a system notification for admins/operators to process (email/push)
        await QueueSystemNotificationAsync(companyId, (byte)NotificationType.AgentOffline, JsonSerializer.Serialize(new { ChatId = chatId, Message = message }));

        // NOTE: sending emails to company admins requires a method to fetch admins which is not present
        // in INotificationRepository. Implement admin retrieval (or provide company admin list) and then:
        // foreach (var admin in admins) await _emailService.SendEmailAsync(admin.Email, title, message);
    }

    public async Task NotifyUnassignedChatAsync(long companyId, long chatId)
    {
        var title = "Unassigned Chat Alert";
        var message = $"Chat (ID: {chatId}) has been waiting for more than 5 minutes.";

        _logger.LogInformation("NotifyUnassignedChatAsync companyId={CompanyId} chatId={ChatId}", companyId, chatId);

        await QueueSystemNotificationAsync(companyId, (byte)NotificationType.UnassignedChat, JsonSerializer.Serialize(new { ChatId = chatId, Message = message }));

        // As above: if you need immediate emails, add a repo/service method to retrieve company admins.
    }

    public async Task QueueSystemNotificationAsync(long companyId, byte notificationType, string payload)
    {
        // Repository expects NotificationType enum, convert from byte.
        await _notificationRepo.AddToSystemQueueAsync(companyId, (NotificationType)notificationType, payload);
        _logger.LogInformation("Queued system notification for company {CompanyId}, type {NotificationType}", companyId, notificationType);
    }
}