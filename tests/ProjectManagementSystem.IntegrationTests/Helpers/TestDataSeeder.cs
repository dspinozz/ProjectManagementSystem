using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Domain.Entities;
using ProjectManagementSystem.Application.Interfaces;
using ProjectManagementSystem.Infrastructure.Data;

namespace ProjectManagementSystem.IntegrationTests.Helpers;

public static class TestDataSeeder
{
    public static async System.Threading.Tasks.Task SeedTestDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Create roles first (if they don't exist)
        var roles = new[] { "Admin", "ProjectManager", "TeamMember" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create test organization
        var organization = new Organization
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Test Organization",
            Description = "Test Organization Description",
            CreatedAt = DateTime.UtcNow
        };

        if (!await context.Organizations.AnyAsync(o => o.Id == organization.Id))
        {
            context.Organizations.Add(organization);
        }
        else
        {
            // Update existing organization
            var existing = await context.Organizations.FindAsync(organization.Id);
            if (existing != null)
            {
                existing.Name = organization.Name;
                existing.Description = organization.Description;
            }
        }

        // Create test workspace
        var workspace = new Workspace
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Test Workspace",
            Description = "Test Workspace Description",
            OrganizationId = organization.Id,
            CreatedAt = DateTime.UtcNow
        };

        if (!await context.Workspaces.AnyAsync(w => w.Id == workspace.Id))
        {
            context.Workspaces.Add(workspace);
        }
        else
        {
            var existing = await context.Workspaces.FindAsync(workspace.Id);
            if (existing != null)
            {
                existing.Name = workspace.Name;
                existing.Description = workspace.Description;
            }
        }

        // Create test project
        var project = new Project
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Test Project",
            Description = "Test Project Description",
            Status = ProjectStatus.InProgress,
            WorkspaceId = workspace.Id,
            CreatedAt = DateTime.UtcNow
        };

        if (!await context.Projects.AnyAsync(p => p.Id == project.Id))
        {
            context.Projects.Add(project);
        }
        else
        {
            var existing = await context.Projects.FindAsync(project.Id);
            if (existing != null)
            {
                existing.Name = project.Name;
                existing.Description = project.Description;
            }
        }

        // Save organization/workspace/project changes first
        await context.SaveChangesAsync();

        // Create test user (if not exists)
        var testUser = await userManager.FindByIdAsync("test-user-id-12345");
        if (testUser == null)
        {
            testUser = new ApplicationUser
            {
                Id = "test-user-id-12345",
                UserName = "testuser@example.com",
                Email = "testuser@example.com",
                EmailConfirmed = true,
                FirstName = "Test",
                LastName = "User",
                OrganizationId = organization.Id,
                WorkspaceId = workspace.Id,
                CreatedAt = DateTime.UtcNow
            };
            var createResult = await userManager.CreateAsync(testUser, "Test1234!");
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create test user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
            // Explicitly save user to database
            await context.SaveChangesAsync();
        }
        
        // Ensure user is in TeamMember role
        if (!await userManager.IsInRoleAsync(testUser, "TeamMember"))
        {
            var roleResult = await userManager.AddToRoleAsync(testUser, "TeamMember");
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to add TeamMember role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
            await context.SaveChangesAsync();
        }

        // Create admin user
        var adminUser = await userManager.FindByIdAsync("admin-user-id-12345");
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                Id = "admin-user-id-12345",
                UserName = "admin@example.com",
                Email = "admin@example.com",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                OrganizationId = organization.Id,
                CreatedAt = DateTime.UtcNow
            };
            var createResult = await userManager.CreateAsync(adminUser, "Admin1234!");
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            }
            // Explicitly save user to database
            await context.SaveChangesAsync();
        }
        
        // Ensure user is in Admin role
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to add Admin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
            await context.SaveChangesAsync();
        }
        
        // Final save to ensure all changes are persisted
        await context.SaveChangesAsync();
    }

    public static Guid TestOrganizationId => Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static Guid TestWorkspaceId => Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static Guid TestProjectId => Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static string TestUserId => "test-user-id-12345";
    public static string AdminUserId => "admin-user-id-12345";
}
