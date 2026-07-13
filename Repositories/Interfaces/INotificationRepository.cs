using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Constants;

namespace SupportChat.Backend.Repositories.Interfaces;

public interface INotificationRepository
{
    // Notifications
    Task<long> CreateNotificationAsync(long companyId, Notification notification, object value, byte newChat);
    Task<IEnumerable<Notification>> GetNotificationsByUserAsync(long userId);
    Task MarkNotificationAsReadAsync(long notificationId);

    // Email Templates
    Task<dynamic> GetEmailTemplateAsync(string templateKey, long? companyId = null);
    Task<long> CreateEmailTemplateAsync(long? companyId, string templateKey, string subject, string body);

    // System Notification Queue
    Task<long> AddToSystemQueueAsync(long companyId, NotificationType notificationType, string payload);
    Task<IEnumerable<dynamic>> GetPendingSystemQueueItemsAsync();
    Task MarkSystemQueueItemProcessedAsync(long queueId);

    // Notification Logs
    Task<long> CreateNotificationLogAsync(long notificationId, NotificationChannel channel, NotificationStatus status, string? errorMessage = null);
    Task<IEnumerable<dynamic>> GetCompanyAdminsAsync(long companyId);
    Task AddSystemQueueItemAsync(long companyId, byte notificationType, string payload);


}