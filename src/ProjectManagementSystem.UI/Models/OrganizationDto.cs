using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.UI.Models;

public class OrganizationDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateOrganizationRequest
{
    [Required(ErrorMessage = "Organization name is required")]
    [MinLength(2, ErrorMessage = "Organization name must be at least 2 characters")]
    [MaxLength(200, ErrorMessage = "Organization name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}

public class UpdateOrganizationRequest
{
    [Required(ErrorMessage = "Organization name is required")]
    [MinLength(2, ErrorMessage = "Organization name must be at least 2 characters")]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
}
