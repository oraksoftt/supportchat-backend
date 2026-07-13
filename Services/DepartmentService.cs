using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Models.Requests;
using SupportChat.Backend.Repositories.Interfaces;
using SupportChat.Backend.Services.Interfaces;

namespace SupportChat.Backend.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _deptRepo;
    private readonly IAuditRepository _auditRepo;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(
        IDepartmentRepository deptRepo,
        IAuditRepository auditRepo,
        ILogger<DepartmentService> logger)
    {
        _deptRepo = deptRepo;
        _auditRepo = auditRepo;
        _logger = logger;
    }

    public async Task<Department> GetByIdAsync(long departmentId)
    {
        var dept = await _deptRepo.GetDepartmentByIdAsync(departmentId);
        if (dept == null)
        {
            throw new InvalidOperationException("Department not found.");
        }

        return dept;
    }

    public async Task<IEnumerable<Department>> GetByCompanyAsync(long companyId)
    {
        return await _deptRepo.GetDepartmentsByCompanyAsync(companyId);
    }

    public async Task<long> CreateAsync(CreateDepartmentRequest request)
    {
        var department = new Department
        {
            CompanyId = request.CompanyId,
            Name = request.Name,
            Description = request.Description,
            CreatedBy = request.CreatedBy
        };

        var deptId = await _deptRepo.CreateDepartmentAsync(department);

        await _auditRepo.CreateAuditLogAsync(
            request.CompanyId,
            request.CreatedBy,
            "DepartmentCreated",
            "Department",
            deptId,
            null,
            $"Name={request.Name}",
            null,
            null
        );

        return deptId;
    }

    public async Task UpdateAsync(UpdateDepartmentRequest request)
    {
        var existing = await _deptRepo.GetDepartmentByIdAsync(request.DepartmentId);
        if (existing == null)
        {
            throw new InvalidOperationException("Department not found.");
        }

        var updated = new Department
        {
            Id = request.DepartmentId,
            CompanyId = existing.CompanyId,
            Name = request.Name,
            Description = request.Description,
            ModifiedBy = request.ModifiedBy
        };

        await _deptRepo.UpdateDepartmentAsync(updated);

        await _auditRepo.CreateAuditLogAsync(
            existing.CompanyId,
            request.ModifiedBy,
            "DepartmentUpdated",
            "Department",
            request.DepartmentId,
            System.Text.Json.JsonSerializer.Serialize(existing),
            System.Text.Json.JsonSerializer.Serialize(request),
            null,
            null
        );
    }

    public async Task DeleteAsync(long departmentId, long deletedBy)
    {
        var dept = await _deptRepo.GetDepartmentByIdAsync(departmentId);
        if (dept == null)
        {
            throw new InvalidOperationException("Department not found.");
        }

        await _deptRepo.DeleteDepartmentAsync(departmentId, deletedBy);

        await _auditRepo.CreateAuditLogAsync(
            dept.CompanyId,
            deletedBy,
            "DepartmentDeleted",
            "Department",
            departmentId,
            null,
            $"Deleted by UserId={deletedBy}",
            null,
            null
        );
    }
}