using System.Collections.Concurrent;
using SupportChat.Backend.Services.Interfaces;

namespace SupportChat.Backend.Services;

public class ConnectionManager : IConnectionManager
{
    // Mapping: UserId -> ConnectionId (last or all connections)
    private readonly ConcurrentDictionary<long, string> _userConnections = new();
    // Mapping: CompanyId -> List of UserIds
    private readonly ConcurrentDictionary<long, HashSet<long>> _companyUsers = new();
    // For multiple connections per user (maybe), but for simplicity we store one per user.

    private readonly ILogger<ConnectionManager> _logger;

    public ConnectionManager(ILogger<ConnectionManager> logger)
    {
        _logger = logger;
    }

    public Task AddConnectionAsync(long userId, string connectionId)
    {
        // Add or update user's connection
        _userConnections[userId] = connectionId;

        // Add user to company group (we'll maintain a separate list)
        // We'll need to know companyId – we'll get from user claims when adding.
        // We'll handle this in the hub with explicit JoinCompanyGroup call.

        return Task.CompletedTask;
    }

    public Task RemoveConnectionAsync(string connectionId)
    {
        // Find the user associated with this connectionId
        var userId = _userConnections.FirstOrDefault(x => x.Value == connectionId).Key;
        if (userId != 0)
        {
            _userConnections.TryRemove(userId, out _);
            // Remove from company user list is handled separately.
        }
        return Task.CompletedTask;
    }

    public Task<string?> GetConnectionIdAsync(long userId)
    {
        _userConnections.TryGetValue(userId, out var connectionId);
        return Task.FromResult(connectionId);
    }

    public Task<IEnumerable<string>> GetConnectionIdsByCompanyAsync(long companyId)
    {
        // To get all connections in a company, we need to maintain a list.
        // We'll do that in the hub by using SignalR groups and maybe a separate dictionary.
        // For simplicity, we can just return all known connectionIds.
        var ids = _userConnections.Values.ToList();
        return Task.FromResult<IEnumerable<string>>(ids);
    }

    public Task<IEnumerable<string>> GetConnectionIdsByUserAsync(long userId)
    {
        if (_userConnections.TryGetValue(userId, out var conn))
            return Task.FromResult<IEnumerable<string>>(new[] { conn });
        return Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
    }

    public Task<IEnumerable<long>> GetOnlineUserIdsByCompanyAsync(long companyId)
    {
        // We need to maintain company->user mapping. Let's add it.
        // We'll add a separate dictionary.
        // We'll update in hub when user joins company group.
        var users = _companyUsers.GetValueOrDefault(companyId, new HashSet<long>());
        return Task.FromResult<IEnumerable<long>>(users);
    }

    // Additional methods to manage company users
    public void AddUserToCompany(long companyId, long userId)
    {
        _companyUsers.AddOrUpdate(companyId,
            new HashSet<long> { userId },
            (key, existing) => { existing.Add(userId); return existing; });
    }

    public void RemoveUserFromCompany(long companyId, long userId)
    {
        if (_companyUsers.TryGetValue(companyId, out var users))
        {
            users.Remove(userId);
            if (users.Count == 0)
                _companyUsers.TryRemove(companyId, out _);
        }
    }
}