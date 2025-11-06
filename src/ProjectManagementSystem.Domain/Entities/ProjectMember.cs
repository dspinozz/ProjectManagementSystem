namespace ProjectManagementSystem.Domain.Entities;

public class ProjectMember
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ProjectRole Role { get; set; }
    public DateTime JoinedAt { get; set; }
    
    // Navigation properties
    public Project Project { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}

public enum ProjectRole
{
    ProjectManager,
    TeamMember,
    Viewer
}

