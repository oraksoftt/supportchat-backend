using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Models.Requests;
using SupportChat.Backend.Repositories.Interfaces;
using SupportChat.Backend.Services.Interfaces;

namespace SupportChat.Backend.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepo;
    private readonly IAuditRepository _auditRepo;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICustomerRepository customerRepo,
        IAuditRepository auditRepo,
        ILogger<CustomerService> logger)
    {
        _customerRepo = customerRepo;
        _auditRepo = auditRepo;
        _logger = logger;
    }

    public async Task<Customer> GetByIdAsync(long customerId)
    {
        return await _customerRepo.GetCustomerByIdAsync(customerId);
    }

    public async Task<IEnumerable<Customer>> GetByCompanyAsync(long companyId)
    {
        return await _customerRepo.GetCustomersByCompanyAsync(companyId);
    }

    //public Task<long> CreateOrGetAsync(long companyId, CreateChatRequest request)
    //{
    //    throw new NotImplementedException();
    //}

    public async Task<Customer> CreateOrGetAsync(long companyId,CreateOrGetCustomerRequest request)
    {
        return await _customerRepo.CreateOrGetCustomerAsync(new Customer {
            CompanyId = request.CompanyId,
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            IpAddress = request.IpAddress,
            Country = request.Country,
            City = request.City,
            Device = request.Device,
            Browser = request.Browser,
            OperatingSystem = request.OperatingSystem
        });
    }

    public async Task UpdateAsync(UpdateCustomerRequest request)
    {
        var existing = await _customerRepo.GetCustomerByIdAsync(request.CustomerId);
        if (existing == null)
        {
            throw new InvalidOperationException("Customer not found.");
        }

        await _customerRepo.UpdateCustomerAsync(new Customer { 
           Id= request.CustomerId,
           Name= request.Name,
           Email= request.Email,
           Phone= request.Phone,
           City= request.City,
           Country= request.Country
        });

        await _auditRepo.CreateAuditLogAsync(
            existing.CompanyId,
            null,
            "CustomerUpdated",
            "Customer",
            request.CustomerId,
            System.Text.Json.JsonSerializer.Serialize(existing),
            System.Text.Json.JsonSerializer.Serialize(request),
            null,
            null
        );
    }

    public async Task BlockAsync(long customerId)
    {
        var customer = await _customerRepo.GetCustomerByIdAsync(customerId);
        if (customer == null)
        {
            throw new InvalidOperationException("Customer not found.");
        }

        await _customerRepo.BlockCustomerAsync(customerId);

        await _auditRepo.CreateAuditLogAsync(
            customer.CompanyId,
            null,
            "CustomerBlocked",
            "Customer",
            customerId,
            null,
            "Customer blocked.",
            null,
            null
        );
    }

    public async Task UnblockAsync(long customerId)
    {
        var customer = await _customerRepo.GetCustomerByIdAsync(customerId);
        if (customer == null)
        {
            throw new InvalidOperationException("Customer not found.");
        }

        await _customerRepo.UnblockCustomerAsync(customerId);

        await _auditRepo.CreateAuditLogAsync(
            customer.CompanyId,
            null,
            "CustomerUnblocked",
            "Customer",
            customerId,
            null,
            "Customer unblocked.",
            null,
            null
        );
    }

    public async Task<bool> IsBlockedAsync(long customerId)
    {
        var customer = await _customerRepo.GetCustomerByIdAsync(customerId);
        return customer?.IsBlocked ?? false;
    }

}