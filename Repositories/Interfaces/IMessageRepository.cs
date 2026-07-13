using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Constants;

namespace SupportChat.Backend.Repositories.Interfaces;

public interface IMessageRepository
{
    Task<IEnumerable<Message>> GetMessagesByChatAsync(long chatId);
    Task<long> SendMessageAsync(long chatId, Message message);
    Task MarkMessageAsSeenAsync(long messageId);
    Task<long> AddAttachmentAsync(Attachment attachment);
}