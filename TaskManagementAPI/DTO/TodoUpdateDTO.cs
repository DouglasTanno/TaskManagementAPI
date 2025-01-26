using TaskManagementAPI.Models;

namespace TaskManagementAPI.DTO;

public class TodoUpdateDTO
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TodoStatus? Status { get; set; }
}