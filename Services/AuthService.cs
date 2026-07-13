using Microsoft.AspNetCore.Identity.Data;
using SupportChat.Backend.Helpers;
using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Models.Requests;
using SupportChat.Backend.Models.Responses;
using SupportChat.Backend.Repositories.Interfaces;
using SupportChat.Backend.Services.Interfaces;
using System.Security.Claims;
using System.Linq;

namespace SupportChat.Backend.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepo;
    private readonly ICompanyRepository _companyRepo;
    private readonly IAuditRepository _auditRepo;
    private readonly JwtHelper _jwtHelper;
    private readonly ILogger<AuthService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AuthService(IAuthRepository authRepo, ICompanyRepository companyRepo, IAuditRepository auditRepo,
        JwtHelper jwtHelper, ILogger<AuthService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _authRepo = authRepo;
        _companyRepo = companyRepo;
        _auditRepo = auditRepo;
        _jwtHelper = jwtHelper;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }



    public async Task<AuthResponse> LoginAsync(Models.Requests.LoginRequest request)
    {
        try
        {
            var userDyn = await _authRepo.GetUserByEmailAsync(request.Email);
            if (userDyn == null)
            {
                _logger.LogWarning("Login failed: user not found for email {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var user = MapToUser(userDyn);
            if (user == null)
            {
                _logger.LogWarning("Login failed: user not found for email {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            // Verify password using BCrypt
            if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: invalid password for email {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            // Check if user is active and not deleted
            if (!user.IsActive || user.IsDeleted)
            {
                _logger.LogWarning("Login failed: account inactive for email {Email}", request.Email);
                throw new UnauthorizedAccessException("Your account is not active. Please contact support.");
            }

            // Get user roles from DB. Make null-safe and fallback to 'User' if none found.
            IEnumerable<string> rolesEnumerable = await _authRepo.GetUserRolesAsync(user.Id) ?? Enumerable.Empty<string>();
            var roles = rolesEnumerable.ToList();
            if (roles.Count == 0)
            {
                _logger.LogInformation("No roles found for user {UserId}, assigning default role 'User'", (object)user.Id);
                roles.Add("Agent");
            }

            // Get permissions
            IEnumerable<string> permissionsEnumerable = await _authRepo.GetUserPermissionsAsync(user.Id) ?? Enumerable.Empty<string>();
            var permissions = permissionsEnumerable.ToList();

            // Generate tokens
            var accessToken = _jwtHelper.GenerateAccessToken(user, roles, permissions);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            // Save refresh token
            await _authRepo.CreateRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(30));

            // Update online status
            await _authRepo.SetUserOnlineStatusAsync(user.Id, true);

            // Log audit
            await _auditRepo.CreateAuditLogAsync(
                user.CompanyId,
                user.Id,
                "Login",
                "User",
                user.Id,
                null,
                null,
                //"127.0.0.1", // Should get from HttpContext
                GetClientIpAddress(),
                "System"
            );

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = user.Id,
                Email = user.Email,
                Roles = roles.ToArray(),
                Permissions = permissions.ToArray()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            throw;
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var refreshToken = await _authRepo.GetRefreshTokenAsync(request.RefreshToken);
        if (refreshToken == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }

        var userDyn = await _authRepo.GetUserByIdAsync(refreshToken.UserId);
        if (userDyn == null)
        {
            throw new UnauthorizedAccessException("User is inactive or deleted.");
        }

        var user = MapToUser(userDyn);
        if (!user.IsActive || user.IsDeleted)
            throw new UnauthorizedAccessException("User is inactive or deleted.");

        // Revoke the old refresh token
        await _authRepo.RevokeRefreshTokenAsync(request.RefreshToken);

        /* // Generate new tokens
         var roles = (await _authRepo.GetUserRolesAsync(user.Id))?.ToList() ?? new List<string>();
         if (roles.Count == 0)
         {
             _logger.LogInformation("No roles found for user {UserId} during refresh, assigning default role 'User'", (object)user.Id);
             roles.Add("User");
         }*/
        var rolesResult = await _authRepo.GetUserRolesAsync(user.Id);

        var roles = rolesResult is IEnumerable<string> roleList ? roleList.ToList() : new List<string>();

        if (roles.Count == 0)
        {
            _logger.LogInformation("No roles found for user {UserId} during refresh, assigning default role 'User'", (object)user.Id);
            roles.Add("Agent");
        }
        var permissionsResult = await _authRepo.GetUserPermissionsAsync(user.Id);

        var permissions = permissionsResult is IEnumerable<string> permissionList ? permissionList.ToList() : new List<string>();
        //var permissions = (await _authRepo.GetUserPermissionsAsync(user.Id)).ToList();
        var accessToken = _jwtHelper.GenerateAccessToken(user, roles, permissions);
        var newRefreshToken = _jwtHelper.GenerateRefreshToken();

        await _authRepo.CreateRefreshTokenAsync(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(30));

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            UserId = user.Id,
            Email = user.Email
            ,
            Roles = roles.ToArray(),
            Permissions = permissions.ToArray()
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _authRepo.RevokeRefreshTokenAsync(refreshToken);
        }
        // We don't set IsOnline = false here because SignalR handles that on disconnect
    }

    public async Task<long> RegisterUserAsync(Models.Requests.RegisterRequest request)
    {
        // Validate that company exists and is active
        var company = await _companyRepo.GetCompanyByIdAsync(request.CompanyId);
        if (company == null || !company.IsActive || company.IsDeleted)
        {
            throw new InvalidOperationException("Company does not exist or is inactive.");
        }

        // Check if email is already taken in this company
        var existingUser = await _authRepo.GetUserByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        // Hash password
        var passwordHash = PasswordHelper.HashPassword(request.Password);

        // Create user
        var userId = await _authRepo.CreateUserAsync(
            request.CompanyId,
            request.DepartmentId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Mobile,
            passwordHash,
            string.Empty // Salt not needed with BCrypt
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
        // Audit log
        await _auditRepo.CreateAuditLogAsync(
            request.CompanyId,
            userId,
            "UserCreated",
            "User",
            userId,
            null,
            $"FirstName={request.FirstName}, Email={request.Email}",
            null,
            null
        );

        return userId;
    }

    public async Task SetOnlineStatusAsync(long userId, bool isOnline)
    {
        await _authRepo.SetUserOnlineStatusAsync(userId, isOnline);
    }

    public async Task<bool> ValidateUserAsync(long userId)
    {
        var user = await _authRepo.GetUserByIdAsync(userId);
        return user != null && user.IsActive && !user.IsDeleted;
    }

    public async Task<User?> GetUserByIdAsync(long userId)
    {
        var userDyn = await _authRepo.GetUserByIdAsync(userId);
        if (userDyn == null) return null;
        return MapToUser(userDyn);
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
            PasswordSalt = d.PasswordSalt,
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


    private string? GetClientIpAddress()
    {
        var ipAddress = _httpContextAccessor.HttpContext?
            .Connection
            .RemoteIpAddress?
            .ToString();

        // Local development IPv6 localhost
        if (ipAddress == "::1")
        {
            ipAddress = "127.0.0.1";
        }

        return ipAddress;
    }
}