using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Dtos;

public class CreateUserDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email format is invalid")]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
}
