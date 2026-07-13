using SupportChat.Backend.Models.Domain;

namespace SupportChat.Backend.Repositories.Interfaces;

public interface IDepartmentRepository
{
    Task<Department?> GetDepartmentByIdAsync(long departmentId);
    Task<IEnumerable<Department>> GetDepartmentsByCompanyAsync(long companyId);
    Task<long> CreateDepartmentAsync(Department department);
    Task UpdateDepartmentAsync(Department department);
    Task DeleteDepartmentAsync(long departmentId, long? deletedBy = null);
}