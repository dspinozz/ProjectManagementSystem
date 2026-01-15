using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.API.DTOs;

public class ProjectResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Status { get; set; }
    public string WorkspaceId { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<MemberResponseDto> Members { get; set; } = new();
}

public class CreateProjectRequestDto
{
    [Required(ErrorMessage = "Project name is required")]
    [MinLength(3, ErrorMessage = "Project name must be at least 3 characters")]
    [MaxLength(200, ErrorMessage = "Project name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Workspace ID is required")]
    public string WorkspaceId { get; set; } = string.Empty;
    
    public int Status { get; set; } = 0; // Planning
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class UpdateProjectRequestDto
{
    [Required(ErrorMessage = "Project name is required")]
    [MinLength(3, ErrorMessage = "Project name must be at least 3 characters")]
    [MaxLength(200, ErrorMessage = "Project name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
    
    public int Status { get; set; }
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
