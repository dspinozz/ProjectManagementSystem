using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjectManagementSystem.Domain.Entities;
using ProjectManagementSystem.Application.Interfaces;
using TaskEntity = ProjectManagementSystem.Domain.Entities.Task;

namespace ProjectManagementSystem.Application.Services;

public interface ITaskService
{
    System.Threading.Tasks.Task<TaskEntity?> GetByIdAsync(Guid id);
    System.Threading.Tasks.Task<IEnumerable<TaskEntity>> GetByProjectIdAsync(Guid projectId);
    System.Threading.Tasks.Task<TaskEntity> CreateAsync(TaskEntity task, string userId);
    System.Threading.Tasks.Task<TaskEntity> UpdateAsync(TaskEntity task, string userId);
    System.Threading.Tasks.Task<bool> DeleteAsync(Guid id, string userId);
}

public class TaskService : ITaskService
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        IApplicationDbContext context,
        IAuditService auditService,
        ILogger<TaskService> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<TaskEntity?> GetByIdAsync(Guid id)
    {
        return await _context.Tasks
            .Include(t => t.Project)
            .Include(t => t.AssignedTo)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async System.Threading.Tasks.Task<IEnumerable<TaskEntity>> GetByProjectIdAsync(Guid projectId)
    {
        return await _context.Tasks
            .Include(t => t.AssignedTo)
            .Where(t => t.ProjectId == projectId)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<TaskEntity> CreateAsync(TaskEntity task, string userId)
    {
        // Validate project exists
        var projectExists = await _context.Projects
            .AnyAsync(p => p.Id == task.ProjectId);
        
        if (!projectExists)
        {
            throw new KeyNotFoundException($"Project with ID {task.ProjectId} not found");
        }

        // Validate assigned user exists if provided
        if (!string.IsNullOrEmpty(task.AssignedToId))
        {
            var userExists = await _context.Users
                .AnyAsync(u => u.Id == task.AssignedToId.ToString());
            
            if (!userExists)
            {
                throw new KeyNotFoundException($"User with ID {task.AssignedToId} not found");
            }
        }

        task.Id = Guid.NewGuid();
        task.CreatedBy = userId;
        task.CreatedAt = DateTime.UtcNow;

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            nameof(TaskEntity),
            task.Id,
            "Create",
            userId,
            null,
            $"Created task: {task.Title}",
            null);

        _logger.LogInformation("Task created: {TaskId} by {UserId}", task.Id, userId);
        return task;
    }

    public async System.Threading.Tasks.Task<TaskEntity> UpdateAsync(TaskEntity task, string userId)
    {
        var existingTask = await _context.Tasks.FindAsync(task.Id);
        if (existingTask == null)
        {
            throw new KeyNotFoundException($"Task with ID {task.Id} not found");
        }

        // Validate project exists if changed
        if (existingTask.ProjectId != task.ProjectId)
        {
            var projectExists = await _context.Projects
                .AnyAsync(p => p.Id == task.ProjectId);
            
            if (!projectExists)
            {
                throw new KeyNotFoundException($"Project with ID {task.ProjectId} not found");
            }
        }

        // Validate assigned user exists if changed
        if (!string.IsNullOrEmpty(task.AssignedToId) && existingTask.AssignedToId != task.AssignedToId)
        {
            var userExists = await _context.Users
                .AnyAsync(u => u.Id == task.AssignedToId.ToString());
            
            if (!userExists)
            {
                throw new KeyNotFoundException($"User with ID {task.AssignedToId} not found");
            }
        }

        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.Status = task.Status;
        existingTask.Priority = task.Priority;
        existingTask.DueDate = task.DueDate;
        existingTask.ProjectId = task.ProjectId;
        existingTask.AssignedToId = task.AssignedToId;
        existingTask.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            nameof(TaskEntity),
            task.Id,
            "Update",
            userId,
            null,
            $"Updated task: {task.Title}",
            null);

        _logger.LogInformation("Task updated: {TaskId} by {UserId}", task.Id, userId);
        return existingTask;
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(Guid id, string userId)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return false;
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            nameof(TaskEntity),
            id,
            "Delete",
            userId,
            null,
            $"Deleted task: {task.Title}",
            null);

        _logger.LogInformation("Task deleted: {TaskId} by {UserId}", id, userId);
        return true;
    }
}
