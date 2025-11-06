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
}

