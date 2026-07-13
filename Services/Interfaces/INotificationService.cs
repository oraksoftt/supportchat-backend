
using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Models.Requests;

namespace SupportChat.Backend.Services.Interfaces;

public interface INotificationService
{
    Task<long> CreateNotificationAsync(CreateNotificationRequest request);
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(long userId);
    Task MarkAsReadAsync(long notificationId);
    Task NotifyNewChatAsync(long companyId, long chatId, long customerId);
    Task NotifyChatAssignedAsync(long chatId, long agentId);
    Task NotifyAgentOfflineAsync(long companyId, long chatId);
    Task NotifyUnassignedChatAsync(long companyId, long chatId);
    Task QueueSystemNotificationAsync(long companyId, byte notificationType, string payload);
}