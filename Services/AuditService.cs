using SupportChat.Backend.Repositories.Interfaces;
using SupportChat.Backend.Services.Interfaces;

namespace SupportChat.Backend.Services;

public class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepo;

    public AuditService(IAuditRepository auditRepo)
    {
        _auditRepo = auditRepo;
    }

    public async Task<long> CreateAuditLogAsync(long? companyId, long? userId, string action, string entityName,
                                                 long? entityId, string? oldValues, string? newValues,
                                                 string? ipAddress, string? userAgent)
    {
        return await _auditRepo.CreateAuditLogAsync(companyId, userId, action, entityName, entityId, oldValues, newValues, ipAddress, userAgent);
    }
}
