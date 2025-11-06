namespace ProjectManagementSystem.Domain.Entities;

public class UserRole
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public SystemRole Role { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid? WorkspaceId { get; set; }
    public DateTime AssignedAt { get; set; }
    
    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Organization? Organization { get; set; }
    public Workspace? Workspace { get; set; }
}

public enum SystemRole
{
    Admin,
    ProjectManager,
    TeamMember
}

