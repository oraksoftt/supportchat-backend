using Dapper;
using System.Data;
using SupportChat.Backend.Models.Domain;
using SupportChat.Backend.Repositories.Interfaces;

namespace SupportChat.Backend.Repositories;

public class DepartmentRepository : BaseRepository, IDepartmentRepository
{
    public DepartmentRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Department?> GetDepartmentByIdAsync(long departmentId)
    {
        // There is no specific "GetById" stored proc, we can use GetByCompany and filter or add one.
        // For simplicity, we'll write a custom query (or you can add a stored procedure).
        using var conn = CreateConnection();
        const string sql = "SELECT * FROM company.Departments WHERE Id = @Id AND IsDeleted = 0";
        return await conn.QueryFirstOrDefaultAsync<Department>(sql, new { Id = departmentId });
    }

    public async Task<IEnumerable<Department>> GetDepartmentsByCompanyAsync(long companyId)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", companyId);
        return await conn.QueryAsync<Department>("company.usp_Department_GetByCompany", parameters);
    }

    public async Task<long> CreateDepartmentAsync(Department department)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@CompanyId", department.CompanyId);
        parameters.Add("@Name", department.Name);
        parameters.Add("@Description", department.Description);
        parameters.Add("@CreatedBy", department.CreatedBy);
        return await conn.QuerySingleAsync<long>(
            "company.usp_Department_Create",
            parameters,
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task UpdateDepartmentAsync(Department department)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@DepartmentId", department.Id);
        parameters.Add("@Name", department.Name);
        parameters.Add("@Description", department.Description);
        parameters.Add("@ModifiedBy", department.ModifiedBy);
        await conn.ExecuteAsync(
            "company.usp_Department_Update",
            parameters,
            commandType: CommandType.StoredProcedure
        );
    }

    public async Task DeleteDepartmentAsync(long departmentId, long? deletedBy = null)
    {
        using var conn = CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@DepartmentId", departmentId);
        parameters.Add("@DeletedBy", deletedBy);
        await conn.ExecuteAsync(
            "company.usp_Department_Delete",
            parameters,
            commandType: CommandType.StoredProcedure
        );
    }
}