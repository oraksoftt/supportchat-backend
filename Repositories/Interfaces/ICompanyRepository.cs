using SupportChat.Backend.Models.Domain;

namespace SupportChat.Backend.Repositories.Interfaces;

public interface ICompanyRepository
{
    Task<Company?> GetCompanyByIdAsync(long companyId);
    Task<Company?> GetCompanyByEmailAsync(string email);
    Task<IEnumerable<Company>> GetAllCompaniesAsync();
    Task<long> CreateCompanyAsync(Company company);
    Task UpdateCompanyAsync(Company company);
    Task DeleteCompanyAsync(long companyId, long? deletedBy = null);


}