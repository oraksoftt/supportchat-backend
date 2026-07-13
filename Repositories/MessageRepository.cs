using Dapper;
using System.Data;
using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Repositories.Interfaces;

namespace SupportChat.Backend.Repositories;

public class MessageRepository : BaseRepository, IMessageRepository
{
    
    public MessageRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<IEnumerable<Message>> GetMessagesByChatAsync(long chatId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@ChatId", chatId);
        return await conn.QueryAsync<Message>("chat.usp_Message_GetByChat",parameters);
    }
 
    public async Task<long> SendMessageAsync(long chatId,Message message)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@ChatId", chatId);
        parameters.Add("@CompanyId", message.CompanyId);
        parameters.Add("@SenderType", (byte)message.SenderType);
        parameters.Add("@SenderId", message.SenderId);
        parameters.Add("@MessageType", (byte)message.MessageType);
        parameters.Add("@Message", message.MessageText);
        return await conn.QuerySingleAsync<long>("chat.usp_Message_Send",parameters);
    }

    public async Task MarkMessageAsSeenAsync(long messageId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@MessageId", messageId);
        await conn.ExecuteAsync("chat.usp_Message_MarkAsSeen",parameters);
    }

    public async Task<long> AddAttachmentAsync(Attachment attachment)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@MessageId", attachment.MessageId);
        parameters.Add("@CompanyId", attachment.CompanyId);
        parameters.Add("@FileName", attachment.FileName);
        parameters.Add("@OriginalName", attachment.OriginalName);
        parameters.Add("@ContentType", attachment.ContentType);
        parameters.Add("@FileSize", attachment.FileSize);
        parameters.Add("@FileUrl", attachment.FileUrl);
        return await conn.QuerySingleAsync<long>("chat.usp_Attachment_Add",parameters);
    }

  
}