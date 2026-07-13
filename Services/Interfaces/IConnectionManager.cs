namespace SupportChat.Backend.Services.Interfaces;

public interface IConnectionManager
{
    Task AddConnectionAsync(long userId, string connectionId);
    Task RemoveConnectionAsync(string connectionId);
    Task<string?> GetConnectionIdAsync(long userId);
    Task<IEnumerable<string>> GetConnectionIdsByCompanyAsync(long companyId);
    Task<IEnumerable<string>> GetConnectionIdsByUserAsync(long userId);
    Task<IEnumerable<long>> GetOnlineUserIdsByCompanyAsync(long companyId);
}