namespace SupportChat.Backend.Services.Interfaces
{
    public interface IAuditService
    {
        Task<long> CreateAuditLogAsync(long? companyId, long? userId, string action, string entityName,
                                 long? entityId, string? oldValues, string? newValues,
                                 string? ipAddress, string? userAgent);
        //Task<IEnumerable<dynamic>> GetAuditLogsByCompanyAsync(long companyId);
    }
}

