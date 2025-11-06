using ProjectManagementSystem.Domain.Common;

namespace ProjectManagementSystem.Domain.Entities;

public class Task : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public Guid ProjectId { get; set; }
    public string? AssignedToId { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    
    // Navigation properties
    public Project Project { get; set; } = null!;
    public ApplicationUser? AssignedTo { get; set; }
}

public enum TaskStatus
{
    ToDo,
    InProgress,
    InReview,
    Done,
    Cancelled
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Critical
}

