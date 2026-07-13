using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Models.Requests;

namespace SupportChat.Backend.Services.Interfaces;

public interface IDepartmentService
{
    Task<Department> GetByIdAsync(long departmentId);
    Task<IEnumerable<Department>> GetByCompanyAsync(long companyId);
    Task<long> CreateAsync(CreateDepartmentRequest request);
    Task UpdateAsync(UpdateDepartmentRequest request);
    Task DeleteAsync(long departmentId, long deletedBy);
}