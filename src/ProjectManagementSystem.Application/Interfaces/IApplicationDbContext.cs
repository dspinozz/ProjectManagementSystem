using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Domain.Entities;
using TaskEntity = ProjectManagementSystem.Domain.Entities.Task;

namespace ProjectManagementSystem.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Organization> Organizations { get; }
    DbSet<Workspace> Workspaces { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectMember> ProjectMembers { get; }
    DbSet<TaskEntity> Tasks { get; }
    DbSet<ProjectFile> ProjectFiles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<ApplicationUser> Users { get; }
    
    System.Threading.Tasks.Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

