using Microsoft.Data.SqlClient;
using System.Data;

namespace SupportChat.Backend.Repositories;

public abstract class BaseRepository
{
    protected readonly string _connectionString;

    protected BaseRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    protected IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}