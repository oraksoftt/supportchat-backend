using SupportChat.Backend.Models.Domain;

namespace SupportChat.Backend.Repositories.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetCustomerByIdAsync(long customerId);
    Task<IEnumerable<Customer>> GetCustomersByCompanyAsync(long companyId);
    Task<Customer?> GetCustomerByEmailAsync(long companyId, string email);
    Task<Customer> CreateOrGetCustomerAsync(Customer customer);
    Task BlockCustomerAsync(long customerId);
    Task UnblockCustomerAsync(long customerId);
    Task UpdateCustomerAsync(Customer customer);
}