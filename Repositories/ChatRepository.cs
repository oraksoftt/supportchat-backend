using Dapper;
using System.Data;
using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Repositories.Interfaces;

namespace SupportChat.Backend.Repositories;

public class ChatRepository : BaseRepository, IChatRepository
{
    public ChatRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Chat?> GetChatByIdAsync(long chatId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@ChatId", chatId);
        return await conn.QueryFirstOrDefaultAsync<Chat>("chat.usp_Chat_GetById",parameters,commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Chat>> GetChatsByCompanyAsync(long companyId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId);
        return await conn.QueryAsync<Chat>("chat.usp_Chat_GetByCompany", parameters);
    }

    public async Task<long> CreateChatAsync(Chat chat)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", chat.CompanyId);
        parameters.Add("@CustomerId", chat.CustomerId);
        parameters.Add("@DepartmentId", chat.DepartmentId);
        parameters.Add("@Subject", chat.Subject);
        parameters.Add("@Priority", (byte)chat.Priority);
        return await conn.QuerySingleAsync<long>("chat.usp_Chat_Create", parameters);
    }

    public async Task AssignAgentToChatAsync(long chatId, long agentId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@ChatId", chatId);
        parameters.Add("@AgentId", agentId);
        await conn.ExecuteAsync("chat.usp_Chat_AssignAgent", parameters);
    }

    public async Task CloseChatAsync(long chatId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@ChatId", chatId);
        await conn.ExecuteAsync("chat.usp_Chat_Close", parameters);
    }

    public async Task AddTagToChatAsync(long chatId, long tagId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@ChatId", chatId);
        parameters.Add("@TagId", tagId);
        await conn.ExecuteAsync("chat.usp_Tag_AddToChat", parameters);
    }

    public async Task AddChatRatingAsync(long chatId, long companyId, long customerId, byte rating, string? comment = null)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@ChatId", chatId);
        parameters.Add("@CompanyId", companyId);
        parameters.Add("@CustomerId", customerId);
        parameters.Add("@Rating", rating);
        parameters.Add("@Comment", comment);
        await conn.ExecuteAsync("chat.usp_Rating_Add", parameters);
    }

    public async Task<int> CountChatsByCompanyAsync(long companyId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT COUNT(*) FROM chat.Chats WHERE CompanyId = @CompanyId AND IsDeleted = 0";
        return await conn.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId });
    }

    public async Task<int> CountActiveChatsByCompanyAsync(long companyId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT COUNT(*) FROM chat.Chats WHERE CompanyId = @CompanyId AND Status = 2 AND IsDeleted = 0";
        return await conn.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId });
    }

    public async Task<int> CountWaitingChatsByCompanyAsync(long companyId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT COUNT(*) FROM chat.Chats WHERE CompanyId = @CompanyId AND Status = 1 AND IsDeleted = 0";
        return await conn.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId });
    }

    public async Task<int> CountClosedChatsByCompanyAsync(long companyId)
    {
        using var conn = CreateConnection();
        const string sql = "SELECT COUNT(*) FROM chat.Chats WHERE CompanyId = @CompanyId AND Status = 3 AND IsDeleted = 0";
        return await conn.ExecuteScalarAsync<int>(sql, new { CompanyId = companyId });
    }

    public async Task<double> GetAverageResponseTimeAsync(long companyId)
    {
        using var conn = CreateConnection();
        // Average time between chat start and first agent message (or assignment)
        const string sql = @"
        SELECT AVG(CAST(DATEDIFF(MINUTE, c.StartedOn, m.CreatedOn) AS FLOAT)) 
        FROM chat.Chats c
        JOIN chat.Messages m ON m.ChatId = c.Id
        WHERE c.CompanyId = @CompanyId 
          AND m.SenderType = 2 -- Agent message
          AND c.IsDeleted = 0
          AND m.IsDeleted = 0";
        var result = await conn.ExecuteScalarAsync<double?>(sql, new { CompanyId = companyId });
        return result ?? 0;
    }

    public async Task<IEnumerable<Chat>> GetActiveChatsByAgentAsync(long agentId)
    {
        using var conn = CreateConnection();
        const string sql = @"
        SELECT * FROM chat.Chats 
        WHERE AssignedAgentId = @AgentId AND Status != 3 AND IsDeleted = 0
        ORDER BY LastMessageOn DESC";
        return await conn.QueryAsync<Chat>(sql, new { AgentId = agentId });
    }
}