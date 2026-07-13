using SupportChat.Backend.Constants;

namespace SupportChat.Backend.Models.Requests;

public class CreateNotificationRequest
{
    public long CompanyId { get; set; }
    public long? UserId { get; set; }
    public long? CustomerId { get; set; }
    public NotificationType NotificationType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}