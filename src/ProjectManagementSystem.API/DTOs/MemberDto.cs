using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.API.DTOs;

public class MemberResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserFirstName { get; set; } = string.Empty;
    public string UserLastName { get; set; } = string.Empty;
    public int Role { get; set; } // ProjectRole enum value
    public string RoleName { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public class AddMemberRequestDto
{
    [Required(ErrorMessage = "User ID is required")]
    public string UserId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Role is required")]
    [Range(0, 2, ErrorMessage = "Invalid role. Must be 0 (ProjectManager), 1 (TeamMember), or 2 (Viewer)")]
    public int Role { get; set; } // ProjectRole enum value
}

public class UpdateMemberRoleRequestDto
{
    [Required(ErrorMessage = "Role is required")]
    [Range(0, 2, ErrorMessage = "Invalid role. Must be 0 (ProjectManager), 1 (TeamMember), or 2 (Viewer)")]
    public int Role { get; set; } // ProjectRole enum value
}

public class UserSearchResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? OrganizationId { get; set; }
    public string? WorkspaceId { get; set; }
}
