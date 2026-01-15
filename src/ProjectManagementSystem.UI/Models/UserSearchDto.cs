namespace ProjectManagementSystem.UI.Models;

public class UserSearchDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? OrganizationId { get; set; }
    public string? WorkspaceId { get; set; }
    
    public string FullName => $"{FirstName} {LastName}".Trim();
}
