using SupportChat.Backend.Endpoints;
using SupportChat.Backend.Models.Domain;

namespace SupportChat.Backend.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<dynamic> GetUserByEmailAsync(string email);
    Task<long> CreateUserAsync(long companyId, long? departmentId, string firstName, string lastName,
                              string email, string mobile, string passwordHash, string passwordSalt);
    Task SetUserOnlineStatusAsync(long userId, bool isOnline);
    Task CreateRefreshTokenAsync(long userId, string token, DateTime expiresOn);
    Task RevokeRefreshTokenAsync(string token);
    Task<dynamic> GetRefreshTokenAsync(string token);
    Task<dynamic> GetUserByIdAsync(long userId);

    // Returns role names assigned to a user
    Task<IEnumerable<string>> GetUserRolesAsync(long userId);
    Task<IEnumerable<string>> GetUserPermissionsAsync(long userId);
    Task<long?> GetRoleIdByNameAsync(string roleName);
    Task AssignRoleToUserAsync(long userId, long roleId);





    Task<IEnumerable<dynamic>> GetUsersByCompanyAsync(long companyId);
    Task UpdateUserAsync(UpdateUserRequest request);
    Task DeleteUserAsync(long userId, long deletedBy);
}