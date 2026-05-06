using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Dtos;

public class CreateTaskDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "UserId is required")]
    [Range(1, int.MaxValue, ErrorMessage = "UserId must be greater than zero")]
    public int UserId { get; set; }

    public string? AdditionalInfo { get; set; }
}
