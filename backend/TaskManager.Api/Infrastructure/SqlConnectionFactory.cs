using Microsoft.Data.SqlClient;

namespace TaskManager.Api.Infrastructure;

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("TaskDb")
            ?? throw new InvalidOperationException("Connection string 'TaskDb' is not configured.");
    }

    public SqlConnection Create() => new SqlConnection(_connectionString);
}
