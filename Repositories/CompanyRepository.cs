using Dapper;
using System.Data;
using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Repositories.Interfaces;

namespace SupportChat.Backend.Repositories;

public class CompanyRepository : BaseRepository, ICompanyRepository
{
    public CompanyRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Company?> GetCompanyByIdAsync(long companyId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId);
        return await conn.QueryFirstOrDefaultAsync<Company>("company.usp_Company_GetById",parameters,commandType: CommandType.StoredProcedure);
    }
    public async Task<Company?> GetCompanyByEmailAsync(string email)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@email", email);
        return await conn.QueryFirstOrDefaultAsync<Company>("company.usp_Company_GetByEmail", parameters);
    }

    public async Task<IEnumerable<Company>> GetAllCompaniesAsync()
    {
        using var conn = CreateConnection();
        return await conn.QueryAsync<Company>("company.usp_Company_GetAll",commandType: CommandType.StoredProcedure);
    }

    public async Task<long> CreateCompanyAsync(Company company)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@Name", company.Name);
        parameters.Add("@DisplayName", company.DisplayName);
        parameters.Add("@Email", company.Email);
        parameters.Add("@Phone", company.Phone);
        parameters.Add("@Website", company.Website);
        parameters.Add("@LogoUrl", company.LogoUrl);
        parameters.Add("@Timezone", company.Timezone);
        parameters.Add("@DefaultLanguage", company.DefaultLanguage);
        parameters.Add("@SubscriptionPlanId", company.SubscriptionPlanId);
        parameters.Add("@SubscriptionExpiry", company.SubscriptionExpiry);
        parameters.Add("@CreatedBy", company.CreatedBy);
        return await conn.QuerySingleAsync<long>("company.usp_Company_Create", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateCompanyAsync(Company company)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", company.Id);
        parameters.Add("@Name", company.Name);
        parameters.Add("@DisplayName", company.DisplayName);
        parameters.Add("@Email", company.Email);
        parameters.Add("@Phone", company.Phone);
        parameters.Add("@Website", company.Website);
        parameters.Add("@LogoUrl", company.LogoUrl);
        parameters.Add("@Timezone", company.Timezone);
        parameters.Add("@DefaultLanguage", company.DefaultLanguage);
        parameters.Add("@ModifiedBy", company.ModifiedBy);
        await conn.ExecuteAsync("company.usp_Company_Update", parameters, commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteCompanyAsync(long companyId, long? deletedBy = null)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId);
        parameters.Add("@DeletedBy", deletedBy);
        await conn.ExecuteAsync("company.usp_Company_Delete", parameters, commandType: CommandType.StoredProcedure);
    }
}