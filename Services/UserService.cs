// UserService.cs
using SupportChat.Backend.Endpoints;
using SupportChat.Backend.Helpers;
using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Models.Requests;
using SupportChat.Backend.Repositories.Interfaces;
using SupportChat.Backend.Services.Interfaces;

namespace SupportChat.Backend.Services;

public class UserService : IUserService
{
    private readonly IAuthRepository _authRepo;
    private readonly IAuditRepository _auditRepo;
    private readonly ILogger<UserService> _logger;

    public UserService(IAuthRepository authRepo,IAuditRepository auditRepo,ILogger<UserService> logger)
    {
        _authRepo = authRepo;
        _auditRepo = auditRepo;
        _logger = logger;
    }

    public async Task<User> GetByIdAsync(long userId)
    {
        var user = await _authRepo.GetUserByIdAsync(userId);
        if (user == null) return null!;
        // Map dynamic to User (we can use Dapper's automatic mapping)
        return MapToUser(user);
    }

    public async Task<User?> GetCurrentUserAsync(long userId)
    {
        var user = await _authRepo.GetUserByIdAsync(userId);
        if (user == null) return null;
        return MapToUser(user);
    }

    public async Task<IEnumerable<User>> GetUsersByCompanyAsync(long companyId)
    {
        // We'll need a new stored procedure or query: "usp_User_GetByCompany"
        // For now, we can create a quick SQL query or add it to repository.
        var users = await _authRepo.GetUsersByCompanyAsync(companyId);
        return users.Select(MapToUser);
    }

    public async Task<long> CreateUserAsync(CreateUserRequest request)
    {
        // Check if email exists
        var existing = await _authRepo.GetUserByEmailAsync(request.Email);
        if (existing != null)
            throw new InvalidOperationException("Email already exists.");

        var passwordHash = PasswordHelper.HashPassword(request.Password);
        var userId = await _authRepo.CreateUserAsync(
            request.CompanyId,
            request.DepartmentId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Mobile,
            passwordHash,
            string.Empty
        );

        // 3. Assign Roles Dynamic Handling
        if (request.Roles != null && request.Roles.Length > 0)
        {
            foreach (var roleName in request.Roles)
            {
                var roleId = await _authRepo.GetRoleIdByNameAsync(roleName);
                if (roleId.HasValue)
                {
                    await _authRepo.AssignRoleToUserAsync(userId, roleId.Value);
                }
            }
        }
        else
        {
            var defaultRoleId = await _authRepo.GetRoleIdByNameAsync("Agent");
            if (defaultRoleId.HasValue)
            {
                await _authRepo.AssignRoleToUserAsync(userId, defaultRoleId.Value);
            }
        }

        await _auditRepo.CreateAuditLogAsync(
            request.CompanyId,
            null,
            "UserCreated",
            "User",
            userId,
            null,
            $"Email={request.Email}",
            null,
            null
        );

        return userId;
    }

    public async Task UpdateUserAsync(UpdateUserRequest request)
    {
        // We need to update user details – we'll add a stored procedure or direct update.
        // For now, we'll use a SQL update in repository.
        await _authRepo.UpdateUserAsync(request);
    }

    public async Task DeleteUserAsync(long userId, long deletedBy)
    {
        var user = await _authRepo.GetUserByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        await _authRepo.DeleteUserAsync(userId, deletedBy);
    }

    private User MapToUser(dynamic d)
    {
        return new User
        {
            Id = d.Id,
            CompanyId = d.CompanyId,
            DepartmentId = d.DepartmentId,
            FirstName = d.FirstName,
            LastName = d.LastName,
            Email = d.Email,
            Mobile = d.Mobile,
            PasswordHash = d.PasswordHash,
            ProfileImageUrl = d.ProfileImageUrl,
            IsOnline = d.IsOnline,
            LastLogin = d.LastLogin,
            IsActive = d.IsActive,
            CreatedBy = d.CreatedBy,
            CreatedOn = d.CreatedOn,
            ModifiedBy = d.ModifiedBy,
            ModifiedOn = d.ModifiedOn,
            DeletedBy = d.DeletedBy,
            DeletedOn = d.DeletedOn,
            IsDeleted = d.IsDeleted
        };
    }
}