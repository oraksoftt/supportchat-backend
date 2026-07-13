using Dapper;
using System.Data;
using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Repositories.Interfaces;

namespace SupportChat.Backend.Repositories;

public class CustomerRepository : BaseRepository, ICustomerRepository
{
    public CustomerRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Customer?> GetCustomerByIdAsync(long customerId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CustomerId", customerId);
        return await conn.QueryFirstOrDefaultAsync<Customer>("customer.usp_Customer_GetById",parameters);
    }

    public async Task<IEnumerable<Customer>> GetCustomersByCompanyAsync(long companyId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId);
        return await conn.QueryAsync<Customer>("customer.usp_Customer_GetByCompany",parameters);
    }

    public async Task<Customer?> GetCustomerByEmailAsync(long companyId, string email)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId);
        parameters.Add("@Email", email);
        return await conn.QueryFirstOrDefaultAsync<Customer>("customer.usp_Customer_GetByEmail",parameters);
    }

    public async Task<Customer> CreateOrGetCustomerAsync(Customer customer)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", customer.CompanyId);
        parameters.Add("@Name", customer.Name);
        parameters.Add("@Email", customer.Email);
        parameters.Add("@Phone", customer.Phone);
        parameters.Add("@IpAddress", customer.IpAddress);
        parameters.Add("@Country", customer.Country);
        parameters.Add("@City", customer.City);
        parameters.Add("@Device", customer.Device);
        parameters.Add("@Browser", customer.Browser);
        parameters.Add("@OperatingSystem", customer.OperatingSystem);
        return await conn.QuerySingleAsync<Customer>("customer.usp_Customer_CreateOrGet", parameters);
    }

    public async Task BlockCustomerAsync(long customerId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CustomerId", customerId);
        await conn.ExecuteAsync("customer.usp_Customer_Block", parameters);
    }

    public async Task UnblockCustomerAsync(long customerId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CustomerId", customerId);
        await conn.ExecuteAsync("customer.usp_Customer_Unblock", parameters);
    }

    public async Task UpdateCustomerAsync(Customer customer)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CustomerId", customer.Id);
        parameters.Add("@Name", customer.Name);
        parameters.Add("@Email", customer.Email);
        parameters.Add("@Phone", customer.Phone);
        parameters.Add("@City", customer.City);
        parameters.Add("@Country", customer.Country);
        await conn.ExecuteAsync("customer.usp_Customer_Update", parameters);
    }
}