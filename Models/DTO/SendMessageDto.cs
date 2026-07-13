using SupportChat.Backend.Constants;

namespace SupportChat.Backend.Models.DTOs;

public class SendMessageDto
{
    public long ChatId { get; set; }
    public SenderType SenderType { get; set; }
    public MessageType MessageType { get; set; } = MessageType.Text;
    public string Message { get; set; } = string.Empty;
}