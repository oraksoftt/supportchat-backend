using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Models.Requests;

namespace SupportChat.Backend.Services.Interfaces;

public interface IMessageService
{
    Task<Message> SendMessageAsync(SendMessageRequest request);
    Task<IEnumerable<Message>> GetMessagesByChatAsync(long chatId);
    Task MarkMessageAsSeenAsync(long messageId);
    Task<Attachment> AddAttachmentAsync(AddAttachmentRequest request);
}