using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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

namespace ProjectManagementSystem.UnitTests.Services;

public class AuditServiceTests : IDisposable
{
    private readonly IApplicationDbContext _context;
    private readonly AuditService _service;

    public AuditServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new AuditService(_context);
    }

    [Fact]
    public async System.Threading.Tasks.Task LogAsync_ValidData_CreatesAuditLog()
    {
        // Arrange
        var entityType = "Project";
        var entityId = Guid.NewGuid();
        var action = "Create";
        var userId = "user-123";
        var userName = "Test User";
        var changes = "Created new project";
        var ipAddress = "127.0.0.1";

        // Act
        await _service.LogAsync(entityType, entityId, action, userId, userName, changes, ipAddress);

        // Assert
        var auditLog = await _context.AuditLogs.FirstOrDefaultAsync();
        auditLog.Should().NotBeNull();
        auditLog!.EntityType.Should().Be(entityType);
        auditLog.EntityId.Should().Be(entityId);
        auditLog.Action.Should().Be(action);
        auditLog.UserId.Should().Be(userId);
        auditLog.UserName.Should().Be(userName);
        auditLog.Changes.Should().Be(changes);
        auditLog.IpAddress.Should().Be(ipAddress);
        auditLog.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async System.Threading.Tasks.Task LogAsync_NullUserId_StillCreatesLog()
    {
        // Arrange
        var entityType = "Project";
        var entityId = Guid.NewGuid();
        var action = "Delete";

        // Act
        await _service.LogAsync(entityType, entityId, action, null, null, null, null);

        // Assert
        var auditLog = await _context.AuditLogs.FirstOrDefaultAsync();
        auditLog.Should().NotBeNull();
        auditLog!.UserId.Should().BeNull();
        auditLog.UserName.Should().BeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task LogAsync_MultipleLogs_CreatesMultipleEntries()
    {
        // Arrange
        var entityType = "Project";
        var entityId = Guid.NewGuid();

        // Act
        await _service.LogAsync(entityType, entityId, "Create", "user1", "User1", "Created", null);
        await _service.LogAsync(entityType, entityId, "Update", "user1", "User1", "Updated", null);
        await _service.LogAsync(entityType, entityId, "Delete", "user2", "User2", "Deleted", null);

        // Assert
        var logs = await _context.AuditLogs.ToListAsync();
        logs.Should().HaveCount(3);
        logs[0].Action.Should().Be("Create");
        logs[1].Action.Should().Be("Update");
        logs[2].Action.Should().Be("Delete");
    }

    public void Dispose()
    {
        ((ApplicationDbContext)_context).Dispose();
    }
}

