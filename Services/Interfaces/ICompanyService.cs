using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Models.Requests;

namespace SupportChat.Backend.Services.Interfaces;

public interface ICompanyService
{
    Task<Company> GetByIdAsync(long companyId);
    Task<IEnumerable<Company>> GetAllAsync();
    Task<long> CreateAsync(CreateCompanyRequest request);
    Task UpdateAsync(UpdateCompanyRequest request);
    Task DeleteAsync(long companyId, long deletedBy);
}