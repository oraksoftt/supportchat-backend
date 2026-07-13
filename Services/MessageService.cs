using SupportChat.Backend.Constants;
using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Models.Requests;
using SupportChat.Backend.Repositories.Interfaces;
using SupportChat.Backend.Services.Interfaces;

namespace SupportChat.Backend.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _messageRepo;
    private readonly IChatRepository _chatRepo;
    private readonly IAuditRepository _auditRepo;
    private readonly ILogger<MessageService> _logger;

    public MessageService(
        IMessageRepository messageRepo,
        IChatRepository chatRepo,
        IAuditRepository auditRepo,
        ILogger<MessageService> logger)
    {
        _messageRepo = messageRepo;
        _chatRepo = chatRepo;
        _auditRepo = auditRepo;
        _logger = logger;
    }

    public async Task<Message> SendMessageAsync(SendMessageRequest request)
    {
        // Validate chat exists and is not closed
        var chat = await _chatRepo.GetChatByIdAsync(request.ChatId);
        if (chat == null)
        {
            throw new InvalidOperationException("Chat not found.");
        }

        if (chat.Status == ChatStatus.Closed)
        {
            throw new InvalidOperationException("Chat is closed. Cannot send messages.");
        }

        // Validate sender
        if (request.SenderType == SenderType.Agent)
        {
            // Ensure agent is assigned to this chat or is admin
            // We'll check if the agent is the assigned agent or has permission
        }

        // Build domain message and send via repository
        var messageEntity = new Message
        {
            ChatId = request.ChatId,
            CompanyId = chat.CompanyId,
            SenderType = request.SenderType,
            SenderId = request.SenderId,
            MessageType = request.MessageType,
            MessageText = request.Message,
            CreatedOn = DateTime.UtcNow
        };

        // Repository API expects a Message object (interface definition)
        var messageId = await _messageRepo.SendMessageAsync(request.ChatId, messageEntity);

        // Persisted id -> set on returned object
        messageEntity.Id = messageId;

        // Audit
        await _auditRepo.CreateAuditLogAsync(
            chat.CompanyId,
            request.SenderId,
            "MessageSent",
            "Message",
            messageId,
            null,
            $"ChatId={request.ChatId}, MessageType={request.MessageType}",
            null,
            null
        );

        return messageEntity;
    }

    public async Task<IEnumerable<Message>> GetMessagesByChatAsync(long chatId)
    {
        return await _messageRepo.GetMessagesByChatAsync(chatId);
    }

    public async Task MarkMessageAsSeenAsync(long messageId)
    {
        await _messageRepo.MarkMessageAsSeenAsync(messageId);
    }

    public async Task<Attachment> AddAttachmentAsync(AddAttachmentRequest request)
    {
        // AddAttachmentRequest does not include ChatId, so skip chat validation here.
        // Build attachment domain object and pass to repository
        var attachment = new Attachment
        {
            MessageId = request.MessageId,
            CompanyId = request.CompanyId,
            FileName = request.FileName,
            OriginalName = request.OriginalName,
            ContentType = request.ContentType,
            FileSize = request.FileSize,
            FileUrl = request.FileUrl
        };

        var attachmentId = await _messageRepo.AddAttachmentAsync(attachment);
        attachment.Id = attachmentId;

        return attachment;
    }
}