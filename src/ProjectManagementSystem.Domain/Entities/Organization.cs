using ProjectManagementSystem.Domain.Common;

namespace ProjectManagementSystem.Domain.Entities;

public class Organization : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Workspace> Workspaces { get; set; } = new List<Workspace>();
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}

