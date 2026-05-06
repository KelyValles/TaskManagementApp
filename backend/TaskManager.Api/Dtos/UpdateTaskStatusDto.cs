using System.ComponentModel.DataAnnotations;
using TaskStatusEnum = TaskManager.Api.Models.TaskStatus;

namespace TaskManager.Api.Dtos;

public class UpdateTaskStatusDto
{
    [Required(ErrorMessage = "NewStatus is required")]
    [EnumDataType(typeof(TaskStatusEnum), ErrorMessage = "NewStatus must be Pending, InProgress or Done")]
    public TaskStatusEnum NewStatus { get; set; }
}
