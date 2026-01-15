namespace ProjectManagementSystem.UI.Models;

public class MemberDto
{
    public string Id { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserFirstName { get; set; } = string.Empty;
    public string UserLastName { get; set; } = string.Empty;
    public int Role { get; set; } // 0 = ProjectManager, 1 = TeamMember, 2 = Viewer
    public string RoleName { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public class AddMemberRequest
{
    public string UserId { get; set; } = string.Empty;
    public int Role { get; set; } // 0 = ProjectManager, 1 = TeamMember, 2 = Viewer
}

public class UpdateMemberRoleRequest
{
    public int Role { get; set; } // 0 = ProjectManager, 1 = TeamMember, 2 = Viewer
}
