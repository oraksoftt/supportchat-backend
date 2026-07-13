// IUserService.cs
using SupportChat.Backend.Endpoints;
using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Models.Requests;

namespace SupportChat.Backend.Services.Interfaces;

public interface IUserService
{
    Task<User> GetByIdAsync(long userId);
    Task<User?> GetCurrentUserAsync(long userId);
    Task<IEnumerable<User>> GetUsersByCompanyAsync(long companyId);
    Task<long> CreateUserAsync(CreateUserRequest request);
    Task UpdateUserAsync(UpdateUserRequest request);
    Task DeleteUserAsync(long userId, long deletedBy);
}