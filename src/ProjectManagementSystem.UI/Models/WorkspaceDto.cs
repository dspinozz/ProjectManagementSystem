using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.UI.Models;

public class WorkspaceDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OrganizationId { get; set; } = string.Empty;
    public string? OrganizationName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateWorkspaceRequest
{
    [Required(ErrorMessage = "Workspace name is required")]
    [MinLength(2, ErrorMessage = "Workspace name must be at least 2 characters")]
    [MaxLength(200, ErrorMessage = "Workspace name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Organization ID is required")]
    public string OrganizationId { get; set; } = string.Empty;
}

public class UpdateWorkspaceRequest
{
    [Required(ErrorMessage = "Workspace name is required")]
    [MinLength(2, ErrorMessage = "Workspace name must be at least 2 characters")]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
}
