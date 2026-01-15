using ProjectManagementSystem.UI.Models;

namespace ProjectManagementSystem.UI.Services;

public interface IMemberService
{
    Task<List<MemberDto>> GetProjectMembersAsync(string projectId);
    Task<MemberDto> AddMemberAsync(string projectId, AddMemberRequest request);
    Task<MemberDto> UpdateMemberRoleAsync(string projectId, string userId, UpdateMemberRoleRequest request);
    Task<bool> RemoveMemberAsync(string projectId, string userId);
}
