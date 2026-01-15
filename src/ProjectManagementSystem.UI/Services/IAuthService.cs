using ProjectManagementSystem.UI.Models;

namespace ProjectManagementSystem.UI.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetUserRoleAsync();
    Task<UserDto?> GetUserAsync();
}
