using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectManagementSystem.Domain.Entities;
using ProjectManagementSystem.Application.Interfaces;

namespace ProjectManagementSystem.Application.Services;

public interface IProjectService
{
    Task<Project?> GetByIdAsync(Guid id);
    Task<IEnumerable<Project>> GetAllAsync(Guid? workspaceId = null);
    Task<Project> CreateAsync(Project project, string userId);
    Task<Project> UpdateAsync(Project project, string userId);
    Task<bool> DeleteAsync(Guid id, string userId);
    
    // Member management
    Task<IEnumerable<ProjectMember>> GetProjectMembersAsync(Guid projectId);
    Task<ProjectMember> AddMemberAsync(Guid projectId, string userId, ProjectRole role, string addedByUserId);
    Task<bool> RemoveMemberAsync(Guid projectId, string userId, string removedByUserId);
    Task<ProjectMember> UpdateMemberRoleAsync(Guid projectId, string userId, ProjectRole newRole, string updatedByUserId);
    Task<bool> IsUserProjectMemberAsync(Guid projectId, string userId);
    Task<ProjectRole?> GetUserProjectRoleAsync(Guid projectId, string userId);
}

public class ProjectService : IProjectService
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        IApplicationDbContext context,
        IAuditService auditService,
        IFileStorageService fileStorageService,
        ILogger<ProjectService> logger)
    {
        _context = context;
        _auditService = auditService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<Project?> GetByIdAsync(Guid id)
    {
        return await _context.Projects
            .Include(p => p.Workspace)
            .Include(p => p.Members)
                .ThenInclude(m => m.User)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Project>> GetAllAsync(Guid? workspaceId = null)
    {
        var query = _context.Projects
            .Include(p => p.Workspace)
            .AsQueryable();

        if (workspaceId.HasValue)
        {
            query = query.Where(p => p.WorkspaceId == workspaceId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<Project> CreateAsync(Project project, string userId)
    {
        // Validate workspace exists
        var workspaceExists = await _context.Workspaces
            .AnyAsync(w => w.Id == project.WorkspaceId);
        
        if (!workspaceExists)
        {
            throw new KeyNotFoundException($"Workspace with ID {project.WorkspaceId} not found");
        }

        project.Id = Guid.NewGuid();
        project.CreatedBy = userId;
        project.CreatedAt = DateTime.UtcNow;

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Automatically add creator as ProjectManager
        try
        {
            var creatorMember = new ProjectMember
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                UserId = userId,
                Role = ProjectRole.ProjectManager,
                JoinedAt = DateTime.UtcNow
            };
            _context.ProjectMembers.Add(creatorMember);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Project creator added as member: {ProjectId}, User: {UserId}", project.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to add project creator as member: {ProjectId}, User: {UserId}", project.Id, userId);
            // Don't fail project creation if member addition fails
        }

        await _auditService.LogAsync(
            nameof(Project),
            project.Id,
            "Create",
            userId,
            null,
            $"Created project: {project.Name}",
            null
        );

        _logger.LogInformation("Project created: {ProjectId} by {UserId}", project.Id, userId);
        return project;
    }

    public async Task<Project> UpdateAsync(Project project, string userId)
    {
        var existingProject = await _context.Projects.FindAsync(project.Id);
        if (existingProject == null)
        {
            throw new KeyNotFoundException($"Project with ID {project.Id} not found");
        }

        // Validate workspace exists if changed
        if (existingProject.WorkspaceId != project.WorkspaceId)
        {
            var workspaceExists = await _context.Workspaces
                .AnyAsync(w => w.Id == project.WorkspaceId);
            
            if (!workspaceExists)
            {
                throw new KeyNotFoundException($"Workspace with ID {project.WorkspaceId} not found");
            }
        }

        existingProject.Name = project.Name;
        existingProject.Description = project.Description;
        existingProject.Status = project.Status;
        existingProject.WorkspaceId = project.WorkspaceId;
        existingProject.StartDate = project.StartDate;
        existingProject.EndDate = project.EndDate;
        existingProject.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            nameof(Project),
            project.Id,
            "Update",
            userId,
            null,
            $"Updated project: {project.Name}",
            null
        );

        _logger.LogInformation("Project updated: {ProjectId} by {UserId}", project.Id, userId);
        return existingProject;
    }

    public async Task<bool> DeleteAsync(Guid id, string userId)
    {
        var project = await _context.Projects
            .Include(p => p.Files)
            .FirstOrDefaultAsync(p => p.Id == id);
        
        if (project == null)
        {
            return false;
        }

        // Clean up associated files
        if (project.Files.Any())
        {
            foreach (var file in project.Files)
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(file.FilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete file {FilePath} for project {ProjectId}", file.FilePath, id);
                }
            }
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            nameof(Project),
            id,
            "Delete",
            userId,
            null,
            $"Deleted project: {project.Name}",
            null
        );

        _logger.LogInformation("Project deleted: {ProjectId} by {UserId}", id, userId);
        return true;
    }

    public async Task<IEnumerable<ProjectMember>> GetProjectMembersAsync(Guid projectId)
    {
        return await _context.ProjectMembers
            .Include(m => m.User)
            .Include(m => m.Project)
            .Where(m => m.ProjectId == projectId)
            .ToListAsync();
    }

    public async Task<ProjectMember> AddMemberAsync(Guid projectId, string userId, ProjectRole role, string addedByUserId)
    {
        // Verify project exists
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null)
        {
            throw new KeyNotFoundException($"Project with ID {projectId} not found");
        }

        // Verify user exists
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // Check if member already exists
        var existingMember = await _context.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);
        
        if (existingMember != null)
        {
            throw new InvalidOperationException($"User {userId} is already a member of project {projectId}");
        }

        var member = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            UserId = userId,
            Role = role,
            JoinedAt = DateTime.UtcNow
        };

        _context.ProjectMembers.Add(member);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            nameof(ProjectMember),
            member.Id,
            "Create",
            addedByUserId,
            null,
            $"Added member {user.Email} to project {project.Name} with role {role}",
            null
        );

        _logger.LogInformation("Member added to project: {ProjectId}, User: {UserId}, Role: {Role}", projectId, userId, role);
        
        return await _context.ProjectMembers
            .Include(m => m.User)
            .Include(m => m.Project)
            .FirstOrDefaultAsync(m => m.Id == member.Id) ?? member;
    }

    public async Task<bool> RemoveMemberAsync(Guid projectId, string userId, string removedByUserId)
    {
        var member = await _context.ProjectMembers
            .Include(m => m.User)
            .Include(m => m.Project)
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);
        
        if (member == null)
        {
            return false;
        }

        var projectName = member.Project?.Name ?? projectId.ToString();
        var userEmail = member.User?.Email ?? userId;

        _context.ProjectMembers.Remove(member);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            nameof(ProjectMember),
            member.Id,
            "Delete",
            removedByUserId,
            null,
            $"Removed member {userEmail} from project {projectName}",
            null
        );

        _logger.LogInformation("Member removed from project: {ProjectId}, User: {UserId}", projectId, userId);
        return true;
    }

    public async Task<ProjectMember> UpdateMemberRoleAsync(Guid projectId, string userId, ProjectRole newRole, string updatedByUserId)
    {
        var member = await _context.ProjectMembers
            .Include(m => m.User)
            .Include(m => m.Project)
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);
        
        if (member == null)
        {
            throw new KeyNotFoundException($"Member not found for project {projectId} and user {userId}");
        }

        var oldRole = member.Role;
        member.Role = newRole;
        await _context.SaveChangesAsync();

        var projectName = member.Project?.Name ?? projectId.ToString();
        var userEmail = member.User?.Email ?? userId;

        await _auditService.LogAsync(
            nameof(ProjectMember),
            member.Id,
            "Update",
            updatedByUserId,
            oldRole.ToString(),
            $"Updated member {userEmail} role in project {projectName} from {oldRole} to {newRole}",
            newRole.ToString()
        );

        _logger.LogInformation("Member role updated: {ProjectId}, User: {UserId}, New Role: {Role}", projectId, userId, newRole);
        return member;
    }

    public async Task<bool> IsUserProjectMemberAsync(Guid projectId, string userId)
    {
        return await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId);
    }

    public async Task<ProjectRole?> GetUserProjectRoleAsync(Guid projectId, string userId)
    {
        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);
        
        return member?.Role;
    }
}

