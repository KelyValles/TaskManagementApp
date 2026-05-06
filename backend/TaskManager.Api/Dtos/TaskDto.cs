namespace TaskManager.Api.Dtos;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? AdditionalInfo { get; set; }
    public string? Priority { get; set; }
    public string? DueDate { get; set; }
}
