using TaskManager.Api.Models;
using TaskStatusEnum = TaskManager.Api.Models.TaskStatus;

namespace TaskManager.Api.Repositories;

public class TaskFilter
{
    public int? UserId { get; set; }
    public TaskStatusEnum? Status { get; set; }
    public string? Priority { get; set; }
}

public interface ITaskRepository
{
    Task<TaskItem> CreateAsync(TaskItem task, CancellationToken ct = default);
    Task<IReadOnlyList<TaskItem>> ListAsync(TaskFilter filter, CancellationToken ct = default);
    Task<TaskItem?> GetByIdAsync(int id, CancellationToken ct = default);
    Task UpdateStatusAsync(int id, TaskStatusEnum newStatus, CancellationToken ct = default);
}
