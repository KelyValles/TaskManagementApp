using Microsoft.Data.SqlClient;

namespace TaskManager.Api.Infrastructure;

public interface ISqlConnectionFactory
{
    SqlConnection Create();
}
