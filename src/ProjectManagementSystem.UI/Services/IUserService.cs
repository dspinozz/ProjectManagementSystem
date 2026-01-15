using ProjectManagementSystem.UI.Models;

namespace ProjectManagementSystem.UI.Services;

public interface IUserService
{
    Task<List<UserSearchDto>> SearchUsersAsync(string? search = null, string? organizationId = null, string? workspaceId = null, int page = 1, int pageSize = 50);
    Task<UserSearchDto?> GetUserByIdAsync(string id);
    Task<UserSearchDto?> GetCurrentUserAsync();
}
