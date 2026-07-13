using Dapper;
using SupportChat.Backend.Endpoints;
using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Repositories;
using SupportChat.Backend.Repositories.Interfaces;

public class AuthRepository : BaseRepository, IAuthRepository
{
    public AuthRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<IEnumerable<dynamic>> GetUsersByCompanyAsync(long companyId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId);
        // We'll create a stored procedure or use inline SQL
        const string sql = @"
        SELECT * FROM auth.Users 
        WHERE CompanyId = @CompanyId AND IsDeleted = 0
        ORDER BY Id DESC";
        return await conn.QueryAsync(sql, new { CompanyId = companyId });
    }

    public async Task UpdateUserAsync(UpdateUserRequest request)
    {
        using var conn = CreateConnection();
        const string sql = @"
        UPDATE auth.Users
        SET FirstName = @FirstName,
            LastName = @LastName,
            Email = @Email,
            Mobile = @Mobile,
            IsActive = @IsActive,
            DepartmentId = @DepartmentId,
            ModifiedOn = SYSUTCDATETIME()
        WHERE Id = @UserId AND IsDeleted = 0";
        await conn.ExecuteAsync(sql, new
        {
            request.UserId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Mobile,
            request.IsActive,
            request.DepartmentId
        });
    }

    public async Task DeleteUserAsync(long userId, long deletedBy)
    {
        using var conn = CreateConnection();
        const string sql = @"
        UPDATE auth.Users
        SET IsDeleted = 1,
            DeletedBy = @DeletedBy,
            DeletedOn = SYSUTCDATETIME()
        WHERE Id = @UserId";
        await conn.ExecuteAsync(sql, new { UserId = userId, DeletedBy = deletedBy });
    }

    public async Task<dynamic> GetUserByEmailAsync(string email)
    {
        using var conn = CreateConnection();
        const string sql = @"
        SELECT TOP 1 *
        FROM auth.Users
        WHERE Email = @Email AND IsDeleted = 0";
        return await conn.QueryFirstOrDefaultAsync(sql, new { Email = email });
    }

    public async Task<long> CreateUserAsync(long companyId, long? departmentId, string firstName, string lastName, string email, string mobile, string passwordHash, string passwordSalt)
    {
        using var conn = CreateConnection();
        const string sql = @"
        INSERT INTO auth.Users
            (CompanyId, DepartmentId, FirstName, LastName, Email, Mobile, PasswordHash, PasswordSalt, IsOnline, IsActive, CreatedOn)
        VALUES
            (@CompanyId, @DepartmentId, @FirstName, @LastName, @Email, @Mobile, @PasswordHash, @PasswordSalt, 0, 1, SYSUTCDATETIME());
        SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

        var id = await conn.QuerySingleAsync<long>(sql, new
        {
            CompanyId = companyId,
            DepartmentId = departmentId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Mobile = mobile,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        });

        return id;
    }

    public async Task SetUserOnlineStatusAsync(long userId, bool isOnline)
    {
        using var conn = CreateConnection();
        const string sql = @"
        UPDATE auth.Users
        SET IsOnline = @IsOnline,
            LastLogin = CASE WHEN @IsOnline = 1 THEN SYSUTCDATETIME() ELSE LastLogin END
        WHERE Id = @UserId AND IsDeleted = 0";
        await conn.ExecuteAsync(sql, new { UserId = userId, IsOnline = isOnline });
    }

    public async Task CreateRefreshTokenAsync(long userId, string token, DateTime expiresOn)
    {
        using var conn = CreateConnection();
        const string sql = @"
        INSERT INTO auth.RefreshTokens
            (UserId, Token, ExpiresOn, IsRevoked, CreatedOn)
        VALUES
            (@UserId, @Token, @ExpiresOn, 0, SYSUTCDATETIME());";
        await conn.ExecuteAsync(sql, new { UserId = userId, Token = token, ExpiresOn = expiresOn });
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        using var conn = CreateConnection();
        const string sql = @"
        UPDATE auth.RefreshTokens
        SET IsRevoked = 1,
            RevokedOn = SYSUTCDATETIME()
        WHERE Token = @Token";
        await conn.ExecuteAsync(sql, new { Token = token });
    }       

    public async Task<dynamic> GetRefreshTokenAsync(string token)
    {
        using var conn = CreateConnection();
        const string sql = @"
        SELECT TOP 1 *
        FROM auth.RefreshTokens
        WHERE Token = @Token";
        return await conn.QueryFirstOrDefaultAsync(sql, new { Token = token });
    }

    public async Task<dynamic> GetUserByIdAsync(long userId)
    {
        using var conn = CreateConnection();
        const string sql = @"
        SELECT TOP 1 *
        FROM auth.Users
        WHERE Id = @UserId AND IsDeleted = 0";
        return await conn.QueryFirstOrDefaultAsync(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(long userId)
    {
        using var conn = CreateConnection();
        const string sql = @"
        SELECT r.Name
        FROM auth.UserRoles ur
        INNER JOIN auth.Roles r ON ur.RoleId = r.Id
        WHERE ur.UserId = @UserId";
        return await conn.QueryAsync<string>(sql, new { UserId = userId });
    }

    public async Task<IEnumerable<string>> GetUserPermissionsAsync(long userId)
    {
        using var conn = CreateConnection();
        const string sql = @"
        SELECT DISTINCT p.Name
        FROM auth.UserRoles ur
        INNER JOIN auth.RolePermissions rp ON ur.RoleId = rp.RoleId
        INNER JOIN auth.Permissions p ON rp.PermissionId = p.Id
        WHERE ur.UserId = @UserId";
        return await conn.QueryAsync<string>(sql, new { UserId = userId });
    }

    public async Task<long?> GetRoleIdByNameAsync(string roleName)
    {
        using var conn = CreateConnection();
        const string sql = @"
        SELECT TOP 1 Id FROM auth.Roles WHERE Name = @Name";
        return await conn.QueryFirstOrDefaultAsync<long?>(sql, new { Name = roleName });
    }

    public async Task AssignRoleToUserAsync(long userId, long roleId)
    {
        using var conn = CreateConnection();
        const string sql = @"
        INSERT INTO auth.UserRoles (UserId, RoleId) VALUES (@UserId, @RoleId)";
        await conn.ExecuteAsync(sql, new { UserId = userId, RoleId = roleId });
    }
}