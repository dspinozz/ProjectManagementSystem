using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.UI.Models;

// Response DTO - matches API TaskResponseDto
public class TaskDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; }
    public int Priority { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public string? AssignedToId { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DueDate { get; set; }
}

// Request DTO - matches API CreateTaskRequestDto
public class CreateTaskRequest
{
    [Required(ErrorMessage = "Task title is required")]
    [MinLength(3, ErrorMessage = "Task title must be at least 3 characters")]
    [MaxLength(200, ErrorMessage = "Task title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Project ID is required")]
    public string ProjectId { get; set; } = string.Empty;
    
    [Range(0, 4, ErrorMessage = "Status must be between 0 and 4")]
    public int Status { get; set; } = 0; // ToDo
    
    [Range(0, 3, ErrorMessage = "Priority must be between 0 and 3")]
    public int Priority { get; set; } = 1; // Medium
    
    public string? AssignedToId { get; set; }
    public DateTime? DueDate { get; set; }
}

// Request DTO - matches API UpdateTaskRequestDto
public class UpdateTaskRequest
{
    [Required(ErrorMessage = "Task title is required")]
    [MinLength(3, ErrorMessage = "Task title must be at least 3 characters")]
    [MaxLength(200, ErrorMessage = "Task title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
    
    [Range(0, 4, ErrorMessage = "Status must be between 0 and 4")]
    public int Status { get; set; }
    
    [Range(0, 3, ErrorMessage = "Priority must be between 0 and 3")]
    public int Priority { get; set; }
    
    public string? AssignedToId { get; set; }
    public DateTime? DueDate { get; set; }
}
