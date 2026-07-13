using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Models.Requests;

namespace SupportChat.Backend.Services.Interfaces;

public interface IChatService
{
    Task<Chat> GetByIdAsync(long chatId);
    Task<IEnumerable<Chat>> GetByCompanyAsync(long companyId);
    Task<long> CreateChatAsync(CreateChatRequest request);
    Task AssignAgentAsync(long chatId, long agentId);
    Task CloseChatAsync(long chatId);
    Task TransferChatAsync(long chatId, long newAgentId);
    Task<bool> IsAgentAvailableAsync(long agentId);
    Task<IEnumerable<Chat>> GetAgentActiveChatsAsync(long agentId);
    // Validate a customer token for joining a chat (simple token validation hook)
    Task<bool> ValidateCustomerTokenAsync(long chatId, string token);
}