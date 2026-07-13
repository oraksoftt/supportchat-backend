using SupportChat.Backend.Constants;

namespace SupportChat.Backend.Models.Requests;

public class SendMessageRequest
{
    public long ChatId { get; set; }
    public SenderType SenderType { get; set; }
    public long? SenderId { get; set; }
    public MessageType MessageType { get; set; } = MessageType.Text;
    public string Message { get; set; } = string.Empty;
}

public class AddAttachmentRequest
{
    public long MessageId { get; set; }
    public long CompanyId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileUrl { get; set; } = string.Empty;
}

public class MarkMessageSeenRequest
{
    public long MessageId { get; set; }
}