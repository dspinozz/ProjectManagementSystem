using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectManagementSystem.Application.Services;
using ProjectManagementSystem.Domain.Entities;
using TaskEntity = ProjectManagementSystem.Domain.Entities.Task;
using ProjectManagementSystem.Application.Interfaces;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Services;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Application.Interfaces;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Services;
using ProjectManagementSystem.Infrastructure.Data;
using Xunit;

namespace ProjectManagementSystem.UnitTests.Services;

public class TaskServiceTests : IDisposable
{
    private readonly IApplicationDbContext _context;
    private readonly AuditService _auditService;
    private readonly Mock<ILogger<TaskService>> _loggerMock;
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        
        // Use REAL AuditService, not mock
        _auditService = new AuditService(_context);
        _loggerMock = new Mock<ILogger<TaskService>>();

        _service = new TaskService(
            _context,
            _auditService,
            _loggerMock.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Test Organization",
            CreatedAt = DateTime.UtcNow
        };

        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            Name = "Test Workspace",
            OrganizationId = organization.Id,
            CreatedAt = DateTime.UtcNow
        };

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            WorkspaceId = workspace.Id,
            CreatedAt = DateTime.UtcNow
        };

        _context.Organizations.Add(organization);
        _context.Workspaces.Add(workspace);
        _context.Projects.Add(project);
        ((ApplicationDbContext)_context).SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_ValidTask_ReturnsTask()
    {
        // Arrange
        var project = await _context.Projects.FirstAsync();
        var task = new TaskEntity
        {
            Title = "Test Task",
            Description = "Test Description",
            Status = ProjectManagementSystem.Domain.Entities.TaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            ProjectId = project.Id
        };
        var userId = "test-user-id";

        // Act
        var result = await _service.CreateAsync(task, userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be("Test Task");
        result.CreatedBy.Should().Be(userId);
        
        // Verify audit log was created (REAL implementation)
        var auditLog = await _context.AuditLogs
            .FirstOrDefaultAsync(a => a.EntityId == result.Id && a.Action == "Create");
        auditLog.Should().NotBeNull();
        auditLog!.EntityType.Should().Be(nameof(TaskEntity));
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_InvalidProject_ThrowsException()
    {
        // Arrange
        var task = new TaskEntity
        {
            Title = "Test Task",
            ProjectId = Guid.NewGuid() // Non-existent project
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.CreateAsync(task, userId));
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateAsync_ValidTask_UpdatesTask()
    {
        // Arrange
        var project = await _context.Projects.FirstAsync();
        var task = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Original Title",
            ProjectId = project.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var updatedTask = new TaskEntity
        {
            Id = task.Id,
            Title = "Updated Title",
            Status = ProjectManagementSystem.Domain.Entities.TaskStatus.InProgress,
            Priority = TaskPriority.High,
            ProjectId = project.Id
        };
        var userId = "test-user-id";

        // Act
        var result = await _service.UpdateAsync(updatedTask, userId);

        // Assert
        result.Title.Should().Be("Updated Title");
        result.Status.Should().Be(ProjectManagementSystem.Domain.Entities.TaskStatus.InProgress);
        result.Priority.Should().Be(TaskPriority.High);
        
        // Verify audit log was created (REAL implementation)
        var auditLog = await _context.AuditLogs
            .FirstOrDefaultAsync(a => a.EntityId == task.Id && a.Action == "Update");
        auditLog.Should().NotBeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByProjectIdAsync_ReturnsTasksForProject()
    {
        // Arrange
        var project = await _context.Projects.FirstAsync();
        var task1 = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Task 1",
            ProjectId = project.Id,
            CreatedAt = DateTime.UtcNow
        };
        var task2 = new TaskEntity
        {
            Id = Guid.NewGuid(),
            Title = "Task 2",
            ProjectId = project.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Tasks.AddRange(task1, task2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByProjectIdAsync(project.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Title == "Task 1");
        result.Should().Contain(t => t.Title == "Task 2");
    }

    public void Dispose()
    {
        ((ApplicationDbContext)_context).Dispose();
    }
}
