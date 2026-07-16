using SupportChat.Backend.Models.Domain;

namespace SupportChat.Backend.Repositories.Interfaces;

public interface IChatRepository
{

    Task<Chat?> GetChatByIdAsync(long chatId);
    Task<IEnumerable<Chat>> GetChatsByCompanyAsync(long companyId);
    Task<long> CreateChatAsync(Chat chat);
    Task AssignAgentToChatAsync(long chatId, long agentId);
    Task CloseChatAsync(long chatId, long agentId);
    Task AddTagToChatAsync(long chatId, long tagId);
    Task AddChatRatingAsync(long chatId, long companyId, long customerId, byte rating, string? comment = null);


    // Task<Chat> GetByIdAsync(long chatId);
    //Task<IEnumerable<Chat>> GetByCompanyAsync(long companyId);
    //Task AssignAgentAsync(long chatId, long agentId);
    //Task CloseAsync(long chatId);
    //Task UpdateLastMessageTimeAsync(long chatId);
    //Task<IEnumerable<Chat>> GetActiveChatsByAgentAsync(long agentId);

    // Dashboard stats
    Task<int> CountChatsByCompanyAsync(long companyId);
    Task<int> CountActiveChatsByCompanyAsync(long companyId);
    Task<int> CountWaitingChatsByCompanyAsync(long companyId);
    Task<int> CountClosedChatsByCompanyAsync(long companyId);
    Task<double> GetAverageResponseTimeAsync(long companyId);
}