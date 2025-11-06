namespace ProjectManagementSystem.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty; // Create, Update, Delete
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Changes { get; set; } // JSON string of changes
    public string? IpAddress { get; set; }
    public DateTime Timestamp { get; set; }
}

