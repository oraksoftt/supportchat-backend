using Dapper;
using System.Data;
using SupportChat.Backend.Repositories.Interfaces;

namespace SupportChat.Backend.Repositories;

public class AuditRepository : BaseRepository, IAuditRepository
{
    public AuditRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<long> CreateAuditLogAsync(long? companyId, long? userId, string action, string entityName,
                                                long? entityId, string? oldValues, string? newValues,
                                                string? ipAddress, string? userAgent)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId);
        parameters.Add("@UserId", userId);
        parameters.Add("@Action", action);
        parameters.Add("@EntityName", entityName);
        parameters.Add("@EntityId", entityId);
        parameters.Add("@OldValues", oldValues);
        parameters.Add("@NewValues", newValues);
        parameters.Add("@IpAddress", ipAddress);
        parameters.Add("@UserAgent", userAgent);

        // Stored proc name follows project convention; adjust if different.
        return await conn.QuerySingleAsync<long>("system.usp_AuditLog_Create", parameters);
    }

    public async Task<IEnumerable<dynamic>> GetAuditLogsByCompanyAsync(long companyId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId);
        return await conn.QueryAsync("system.usp_AuditLog_GetByCompany", parameters, commandType: CommandType.StoredProcedure);
    }
}