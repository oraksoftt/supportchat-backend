namespace SupportChat.Backend.Repositories.Interfaces;

public interface IAuditRepository
{
    Task<long> CreateAuditLogAsync(long? companyId, long? userId, string action, string entityName,
                                   long? entityId, string? oldValues, string? newValues,
                                   string? ipAddress, string? userAgent);
    Task<IEnumerable<dynamic>> GetAuditLogsByCompanyAsync(long companyId);
}