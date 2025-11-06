using System.Security.Claims;

namespace ProjectManagementSystem.API.Helpers;

public static class UserContextHelper
{
    public static string? GetUserId(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public static Guid? GetOrganizationId(ClaimsPrincipal user)
    {
        var orgId = user.FindFirstValue("OrganizationId");
        return string.IsNullOrEmpty(orgId) ? null : Guid.Parse(orgId);
    }

    public static Guid? GetWorkspaceId(ClaimsPrincipal user)
    {
        var workspaceId = user.FindFirstValue("WorkspaceId");
        return string.IsNullOrEmpty(workspaceId) ? null : Guid.Parse(workspaceId);
    }

    public static bool IsInRole(ClaimsPrincipal user, string role)
    {
        return user.IsInRole(role);
    }
}

