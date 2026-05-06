using System.Data;
using Microsoft.Data.SqlClient;
using TaskManager.Api.Infrastructure;
using TaskManager.Api.Models;

namespace TaskManager.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ISqlConnectionFactory _factory;

    public UserRepository(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<User> CreateAsync(User user, CancellationToken ct = default)
    {
        const string sql = @"
            INSERT INTO Users (Name, Email)
            OUTPUT INSERTED.Id
            VALUES (@Name, @Email);";

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = user.Name;
        cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = user.Email;

        var newId = (int)(await cmd.ExecuteScalarAsync(ct))!;
        user.Id = newId;
        return user;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = "SELECT Id, Name, Email FROM Users ORDER BY Id;";

        var users = new List<User>();
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            users.Add(new User
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2)
            });
        }
        return users;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        const string sql = "SELECT 1 FROM Users WHERE Id = @Id;";

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        var result = await cmd.ExecuteScalarAsync(ct);
        return result is not null;
    }
}
