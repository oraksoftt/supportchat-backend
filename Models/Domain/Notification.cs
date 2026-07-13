using SupportChat.Backend.Constants;

namespace SupportChat.Backend.Models.Domain;

public class Notification
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public long? UserId { get; set; }
    public long? CustomerId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedOn { get; set; }
}