using SupportChat.Backend.Constants;

namespace SupportChat.Backend.Models.Domain;

public class Message
{
    public long Id { get; set; }
    public long ChatId { get; set; }
    public long CompanyId { get; set; }
    public SenderType SenderType { get; set; }
    public long? SenderId { get; set; } // can be agent or customer id based on type
    public MessageType MessageType { get; set; }
    public string? MessageText { get; set; } // renamed to avoid keyword conflict
    public bool IsEdited { get; set; }
    public DateTime? EditedOn { get; set; }
    public DateTime? DeliveredOn { get; set; }
    public DateTime? SeenOn { get; set; }
    public DateTime CreatedOn { get; set; }
    public bool IsDeleted { get; set; }
}