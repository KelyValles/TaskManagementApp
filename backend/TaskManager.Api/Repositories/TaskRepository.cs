using System.Data;
using Microsoft.Data.SqlClient;
using TaskManager.Api.Infrastructure;
using TaskManager.Api.Models;
using TaskStatusEnum = TaskManager.Api.Models.TaskStatus;

namespace TaskManager.Api.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ISqlConnectionFactory _factory;

    public TaskRepository(ISqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken ct = default)
    {
        const string sql = @"
            INSERT INTO Tasks (Title, UserId, Status, AdditionalInfo)
            OUTPUT INSERTED.Id, INSERTED.CreatedAt
            VALUES (@Title, @UserId, @Status, @AdditionalInfo);";

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 200).Value = task.Title;
        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = task.UserId;
        cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = task.Status.ToString();
        cmd.Parameters.Add("@AdditionalInfo", SqlDbType.NVarChar, -1).Value =
            (object?)task.AdditionalInfo ?? DBNull.Value;

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            task.Id = reader.GetInt32(0);
            task.CreatedAt = reader.GetDateTime(1);
        }
        return task;
    }

    public async Task<IReadOnlyList<TaskItem>> ListAsync(TaskFilter filter, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT Id, Title, UserId, Status, CreatedAt, AdditionalInfo
            FROM Tasks
            WHERE (@UserId IS NULL OR UserId = @UserId)
              AND (@Status IS NULL OR Status = @Status)
              AND (@Priority IS NULL OR JSON_VALUE(AdditionalInfo, '$.priority') = @Priority)
            ORDER BY CreatedAt DESC;";

        var tasks = new List<TaskItem>();
        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@UserId", SqlDbType.Int).Value =
            (object?)filter.UserId ?? DBNull.Value;
        cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value =
            filter.Status.HasValue ? filter.Status.Value.ToString() : DBNull.Value;
        cmd.Parameters.Add("@Priority", SqlDbType.NVarChar, 50).Value =
            (object?)filter.Priority ?? DBNull.Value;

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            tasks.Add(MapTask(reader));
        }
        return tasks;
    }

    public async Task<TaskItem?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT Id, Title, UserId, Status, CreatedAt, AdditionalInfo
            FROM Tasks
            WHERE Id = @Id;";

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (await reader.ReadAsync(ct))
        {
            return MapTask(reader);
        }
        return null;
    }

    public async Task UpdateStatusAsync(int id, TaskStatusEnum newStatus, CancellationToken ct = default)
    {
        const string sql = "UPDATE Tasks SET Status = @Status WHERE Id = @Id;";

        await using var conn = _factory.Create();
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = newStatus.ToString();
        cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static TaskItem MapTask(SqlDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Title = reader.GetString(1),
        UserId = reader.GetInt32(2),
        Status = Enum.Parse<TaskStatusEnum>(reader.GetString(3)),
        CreatedAt = reader.GetDateTime(4),
        AdditionalInfo = reader.IsDBNull(5) ? null : reader.GetString(5)
    };
}
