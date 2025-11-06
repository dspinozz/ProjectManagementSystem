namespace ProjectManagementSystem.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(string entityType, Guid entityId, string action, string? userId, string? userName, string? changes, string? ipAddress);
}

