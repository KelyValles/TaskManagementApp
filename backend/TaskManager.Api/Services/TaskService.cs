using System.Text.Json;
using TaskManager.Api.Dtos;
using TaskManager.Api.Exceptions;
using TaskManager.Api.Models;
using TaskManager.Api.Repositories;
using TaskStatusEnum = TaskManager.Api.Models.TaskStatus;

namespace TaskManager.Api.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;

    public TaskService(ITaskRepository taskRepository, IUserRepository userRepository)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
    }

    public async Task<TaskDto> CreateAsync(CreateTaskDto dto, CancellationToken ct = default)
    {
        if (!await _userRepository.ExistsAsync(dto.UserId, ct))
        {
            throw new NotFoundException($"User {dto.UserId} does not exist.");
        }

        if (!string.IsNullOrWhiteSpace(dto.AdditionalInfo) && !IsValidJson(dto.AdditionalInfo))
        {
            throw new InvalidJsonException("La información adicional debe ser un documento JSON válido.");
        }

        var entity = new TaskItem
        {
            Title = dto.Title.Trim(),
            UserId = dto.UserId,
            Status = TaskStatusEnum.Pending,
            AdditionalInfo = string.IsNullOrWhiteSpace(dto.AdditionalInfo) ? null : dto.AdditionalInfo
        };

        var created = await _taskRepository.CreateAsync(entity, ct);
        return ToDto(created);
    }

    public async Task<IReadOnlyList<TaskDto>> ListAsync(TaskFilter filter, CancellationToken ct = default)
    {
        var tasks = await _taskRepository.ListAsync(filter, ct);
        return tasks.Select(ToDto).ToList();
    }

    public async Task<TaskDto> ChangeStatusAsync(int id, TaskStatusEnum newStatus, CancellationToken ct = default)
    {
        var current = await _taskRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Task {id} does not exist.");

        if (current.Status == TaskStatusEnum.Pending && newStatus == TaskStatusEnum.Done)
        {
            throw new BusinessRuleException(
                "No se permite cambiar una tarea directamente de Pending a Done.");
        }

        if (current.Status == newStatus)
        {
            return ToDto(current);
        }

        await _taskRepository.UpdateStatusAsync(id, newStatus, ct);
        current.Status = newStatus;
        return ToDto(current);
    }

    private static TaskDto ToDto(TaskItem task)
    {
        string? priority = null;
        string? dueDate = null;

        if (!string.IsNullOrWhiteSpace(task.AdditionalInfo))
        {
            try
            {
                using var doc = JsonDocument.Parse(task.AdditionalInfo);
                if (doc.RootElement.TryGetProperty("priority", out var p))
                    priority = p.ValueKind == JsonValueKind.String ? p.GetString() : p.ToString();
                if (doc.RootElement.TryGetProperty("dueDate", out var d))
                    dueDate = d.ValueKind == JsonValueKind.String ? d.GetString() : d.ToString();
            }
            catch (JsonException)
            {
            }
        }

        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            UserId = task.UserId,
            Status = task.Status.ToString(),
            CreatedAt = task.CreatedAt,
            AdditionalInfo = task.AdditionalInfo,
            Priority = priority,
            DueDate = dueDate
        };
    }

    private static bool IsValidJson(string candidate)
    {
        try
        {
            using var _ = JsonDocument.Parse(candidate);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
