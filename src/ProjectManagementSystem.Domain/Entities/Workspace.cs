using ProjectManagementSystem.Domain.Common;

namespace ProjectManagementSystem.Domain.Entities;

public class Workspace : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid OrganizationId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Organization Organization { get; set; } = null!;
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}

