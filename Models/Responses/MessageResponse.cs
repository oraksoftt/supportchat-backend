using SupportChat.Backend.Constants;

namespace SupportChat.Backend.Models.Responses;

public class MessageResponse
{
    public long Id { get; set; }
    public long ChatId { get; set; }
    public SenderType SenderType { get; set; }
    public long? SenderId { get; set; }
    public string? SenderName { get; set; }
    public MessageType MessageType { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedOn { get; set; }
    public bool IsEdited { get; set; }
    public bool IsDelivered { get; set; }
    public bool IsSeen { get; set; }
}