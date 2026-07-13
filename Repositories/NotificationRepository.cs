using Dapper;
using SupportChat.Backend.Constants;
using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Repositories.Interfaces;
using System.Data;

namespace SupportChat.Backend.Repositories;

public class NotificationRepository : BaseRepository, INotificationRepository
{
    public NotificationRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<long> CreateNotificationAsync(Notification notification)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", notification.CompanyId);
        parameters.Add("@UserId", notification.UserId);
        parameters.Add("@CustomerId", notification.CustomerId);
        parameters.Add("@Type", (byte)notification.Type);
        parameters.Add("@Title", notification.Title);
        parameters.Add("@Message", notification.Message);
        return await conn.QuerySingleAsync<long>("notification.usp_Notification_Create",parameters);
    }

    public async Task<long> CreateNotificationAsync(long companyId, Notification notification, object value, byte newChat)
    {
        
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId);
        parameters.Add("@UserId", notification.UserId);
        parameters.Add("@CustomerId", notification.CustomerId);
        parameters.Add("@Type", (byte)notification.Type);
        parameters.Add("@Title", notification.Title);
        parameters.Add("@Message", notification.Message);
        parameters.Add("@Value", value);
        parameters.Add("@NewChat", newChat);
        return await conn.QuerySingleAsync<long>("notification.usp_Notification_Create", parameters);
    }

    public async Task<IEnumerable<Notification>> GetNotificationsByUserAsync(long userId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@UserId", userId);
        return await conn.QueryAsync<Notification>("notification.usp_Notification_GetByUser", parameters);
    }

    public async Task MarkNotificationAsReadAsync(long notificationId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@NotificationId", notificationId);
        await conn.ExecuteAsync("notification.usp_Notification_MarkAsRead", parameters);
    }

    public async Task<dynamic> GetEmailTemplateAsync(string templateKey, long? companyId = null)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@TemplateKey", templateKey);
        parameters.Add("@CompanyId", companyId);
        return await conn.QueryFirstOrDefaultAsync("notification.usp_EmailTemplate_GetByKey",parameters,commandType: CommandType.StoredProcedure);
    }

    public async Task<long> CreateEmailTemplateAsync(long? companyId, string templateKey, string subject, string body)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId);
        parameters.Add("@TemplateKey", templateKey);
        parameters.Add("@Subject", subject);
        parameters.Add("@Body", body);
        return await conn.QuerySingleAsync<long>("notification.usp_EmailTemplate_Create", parameters);
    }

    public async Task<long> AddToSystemQueueAsync(long companyId, NotificationType notificationType, string payload)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId);
        parameters.Add("@NotificationType", (byte)notificationType);
        parameters.Add("@Payload", payload);
        return await conn.QuerySingleAsync<long>("notification.usp_SystemQueue_Add", parameters);
    }

    public async Task AddSystemQueueItemAsync(long companyId, byte notificationType, string payload)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId);
        parameters.Add("@NotificationType", notificationType);
        parameters.Add("@Payload", payload);
        await conn.ExecuteAsync("notification.usp_SystemQueue_Add",parameters,commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<dynamic>> GetPendingSystemQueueItemsAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync("notification.usp_SystemQueue_GetPending",commandType: CommandType.StoredProcedure);
    }

    public async Task MarkSystemQueueItemProcessedAsync(long queueId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@QueueId", queueId);
        await conn.ExecuteAsync("notification.usp_SystemQueue_MarkProcessed",parameters,commandType: CommandType.StoredProcedure);
    }

    public async Task<long> CreateNotificationLogAsync(long notificationId, NotificationChannel channel, NotificationStatus status, string? errorMessage = null)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@NotificationId", notificationId);
        parameters.Add("@Channel", (byte)channel);
        parameters.Add("@Status", (byte)status);
        parameters.Add("@ErrorMessage", errorMessage);
        return await conn.QuerySingleAsync<long>("notification.usp_NotificationLog_Create",parameters,commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<dynamic>> GetCompanyAdminsAsync(long companyId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId);
        return await conn.QueryAsync("notification.usp_CompanyAdmins_Get",parameters,commandType: CommandType.StoredProcedure);
    }
}