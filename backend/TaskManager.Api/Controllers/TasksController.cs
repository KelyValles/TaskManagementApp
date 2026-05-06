using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Dtos;
using TaskManager.Api.Repositories;
using TaskManager.Api.Services;
using TaskStatusEnum = TaskManager.Api.Models.TaskStatus;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _service;

    public TasksController(ITaskService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskDto>> Create(
        [FromBody] CreateTaskDto dto,
        CancellationToken ct)
    {
        var created = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetAll), new { }, created);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> GetAll(
        [FromQuery] int? userId,
        [FromQuery] TaskStatusEnum? status,
        [FromQuery] string? priority,
        CancellationToken ct)
    {
        var filter = new TaskFilter
        {
            UserId = userId,
            Status = status,
            Priority = priority
        };
        var tasks = await _service.ListAsync(filter, ct);
        return Ok(tasks);
    }

    [HttpPut("{id:int}/status")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<TaskDto>> ChangeStatus(
        [FromRoute] int id,
        [FromBody] UpdateTaskStatusDto dto,
        CancellationToken ct)
    {
        var updated = await _service.ChangeStatusAsync(id, dto.NewStatus, ct);
        return Ok(updated);
    }
}
