using TaskManager.Api.Dtos;
using TaskManager.Api.Repositories;
using TaskStatusEnum = TaskManager.Api.Models.TaskStatus;

namespace TaskManager.Api.Services;

public interface ITaskService
{
    Task<TaskDto> CreateAsync(CreateTaskDto dto, CancellationToken ct = default);
    Task<IReadOnlyList<TaskDto>> ListAsync(TaskFilter filter, CancellationToken ct = default);
    Task<TaskDto> ChangeStatusAsync(int id, TaskStatusEnum newStatus, CancellationToken ct = default);
}
