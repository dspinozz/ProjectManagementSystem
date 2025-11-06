namespace ProjectManagementSystem.Domain.Entities;

public class ProjectFile
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public Guid ProjectId { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    
    // Navigation properties
    public Project Project { get; set; } = null!;
}

