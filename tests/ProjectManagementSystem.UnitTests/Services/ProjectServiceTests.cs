using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ProjectManagementSystem.Application.Services;
using ProjectManagementSystem.Domain.Entities;
using ProjectManagementSystem.Application.Interfaces;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Services;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Application.Interfaces;
using ProjectManagementSystem.Infrastructure.Data;
using ProjectManagementSystem.Infrastructure.Services;
using ProjectManagementSystem.Infrastructure.Data;
using Xunit;
using System.IO;

namespace ProjectManagementSystem.UnitTests.Services;

public class ProjectServiceTests : IDisposable
{
    private readonly IApplicationDbContext _context;
    private readonly AuditService _auditService;
    private readonly FileStorageService _fileStorageService;
    private readonly Mock<ILogger<ProjectService>> _loggerMock;
    private readonly ProjectService _service;

    public ProjectServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        
        // Use REAL implementations, not mocks
        _auditService = new AuditService(_context);
        
        var loggerMockForFileStorage = new Mock<ILogger<FileStorageService>>();
        _fileStorageService = new FileStorageService(loggerMockForFileStorage.Object);
        
        _loggerMock = new Mock<ILogger<ProjectService>>();

        _service = new ProjectService(
            _context,
            _auditService,
            _fileStorageService,
            _loggerMock.Object);

        // Seed test data
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

        _context.Organizations.Add(organization);
        _context.Workspaces.Add(workspace);
        ((ApplicationDbContext)_context).SaveChanges();
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_ValidProject_ReturnsProject()
    {
        // Arrange
        var workspace = await _context.Workspaces.FirstAsync();
        var project = new Project
        {
            Name = "Test Project",
            Description = "Test Description",
            Status = ProjectStatus.Planning,
            WorkspaceId = workspace.Id
        };
        var userId = "test-user-id";

        // Act
        var result = await _service.CreateAsync(project, userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Test Project");
        result.CreatedBy.Should().Be(userId);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        
        // Verify audit log was created (REAL implementation)
        var auditLog = await _context.AuditLogs
            .FirstOrDefaultAsync(a => a.EntityId == result.Id && a.Action == "Create");
        auditLog.Should().NotBeNull();
        auditLog!.EntityType.Should().Be(nameof(Project));
        auditLog.UserId.Should().Be(userId);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_InvalidWorkspace_ThrowsException()
    {
        // Arrange
        var project = new Project
        {
            Name = "Test Project",
            WorkspaceId = Guid.NewGuid() // Non-existent workspace
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.CreateAsync(project, userId));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByIdAsync_ExistingProject_ReturnsProject()
    {
        // Arrange
        var workspace = await _context.Workspaces.FirstAsync();
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            WorkspaceId = workspace.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(project.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(project.Id);
        result.Name.Should().Be("Test Project");
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByIdAsync_NonExistentProject_ReturnsNull()
    {
        // Act
        var result = await _service.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateAsync_ValidProject_UpdatesProject()
    {
        // Arrange
        var workspace = await _context.Workspaces.FirstAsync();
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Original Name",
            WorkspaceId = workspace.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var updatedProject = new Project
        {
            Id = project.Id,
            Name = "Updated Name",
            Description = "Updated Description",
            Status = ProjectStatus.InProgress,
            WorkspaceId = workspace.Id
        };
        var userId = "test-user-id";

        // Act
        var result = await _service.UpdateAsync(updatedProject, userId);

        // Assert
        result.Name.Should().Be("Updated Name");
        result.Description.Should().Be("Updated Description");
        result.Status.Should().Be(ProjectStatus.InProgress);
        result.UpdatedAt.Should().NotBeNull();
        
        // Verify audit log was created (REAL implementation)
        var auditLog = await _context.AuditLogs
            .FirstOrDefaultAsync(a => a.EntityId == project.Id && a.Action == "Update");
        auditLog.Should().NotBeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateAsync_NonExistentProject_ThrowsException()
    {
        // Arrange
        var workspace = await _context.Workspaces.FirstAsync();
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            WorkspaceId = workspace.Id
        };
        var userId = "test-user-id";

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.UpdateAsync(project, userId));
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteAsync_ExistingProject_DeletesProject()
    {
        // Arrange
        var workspace = await _context.Workspaces.FirstAsync();
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            WorkspaceId = workspace.Id,
            CreatedAt = DateTime.UtcNow
        };
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAsync(project.Id, "test-user-id");

        // Assert
        result.Should().BeTrue();
        var deletedProject = await _context.Projects.FindAsync(project.Id);
        deletedProject.Should().BeNull();
        
        // Verify audit log was created (REAL implementation)
        var auditLog = await _context.AuditLogs
            .FirstOrDefaultAsync(a => a.EntityId == project.Id && a.Action == "Delete");
        auditLog.Should().NotBeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteAsync_NonExistentProject_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteAsync(Guid.NewGuid(), "test-user-id");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteAsync_ProjectWithFiles_DeletesFiles()
    {
        // Arrange
        var workspace = await _context.Workspaces.FirstAsync();
        // Save file through FileStorageService to get a valid path within base directory
        var fileContent = "test content";
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContent));
        var savedFilePath = await _fileStorageService.SaveFileAsync(stream, "test.txt", "text/plain");
        
        var projectForFiles = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            WorkspaceId = workspace.Id,
            CreatedAt = DateTime.UtcNow,
            Files = new List<ProjectFile>
            {
                new ProjectFile
                {
                    Id = Guid.NewGuid(),
                    FileName = "test.txt",
                    OriginalFileName = "test.txt",
                    FilePath = savedFilePath,
                    ContentType = "text/plain",
                    FileSize = 12,
                    UploadedAt = DateTime.UtcNow
                }
            }
        };
        projectForFiles.Files.First().ProjectId = projectForFiles.Id;
        _context.Projects.Add(projectForFiles);
        await _context.SaveChangesAsync();
        var userId = "test-user-id";

        // Act
        await _service.DeleteAsync(projectForFiles.Id, userId);

        // Assert
        // Verify file was deleted from disk (REAL implementation)
        File.Exists(savedFilePath).Should().BeFalse();
    }

    public void Dispose()
    {
        ((ApplicationDbContext)_context).Dispose();
    }
}
