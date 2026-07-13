using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Models.Requests;

namespace SupportChat.Backend.Services.Interfaces;

public interface ICustomerService
{
    Task<Customer> GetByIdAsync(long customerId);
    Task<IEnumerable<Customer>> GetByCompanyAsync(long companyId);
    Task<Customer> CreateOrGetAsync(long companyId, CreateOrGetCustomerRequest request);
    Task UpdateAsync(UpdateCustomerRequest request);
    Task BlockAsync(long customerId);
    Task UnblockAsync(long customerId);
    Task<bool> IsBlockedAsync(long customerId);



}