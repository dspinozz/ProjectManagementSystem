using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Domain.Entities;
using ProjectManagementSystem.API.Attributes;
using ProjectManagementSystem.Application.Interfaces;
using ProjectManagementSystem.API.Helpers;

namespace ProjectManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAuditService _auditService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IApplicationDbContext context,
        IFileStorageService fileStorageService,
        IAuditService auditService,
        ILogger<FilesController> logger)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _auditService = auditService;
        _logger = logger;
    }

    [HttpPost("upload/{projectId}")]
    [Authorize(Policy = "TeamMemberOrAbove")]
    [FileUploadValidation(maxFileSizeBytes: 10 * 1024 * 1024)] // 10MB max
    public async Task<ActionResult<ProjectFile>> UploadFile(Guid projectId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        var project = await _context.Projects.FindAsync(projectId);
        if (project == null)
        {
            return NotFound("Project not found");
        }

        var userId = UserContextHelper.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            using var stream = file.OpenReadStream();
            var filePath = await _fileStorageService.SaveFileAsync(stream, file.FileName, file.ContentType);

            var projectFile = new ProjectFile
            {
                Id = Guid.NewGuid(),
                FileName = Path.GetFileName(filePath),
                OriginalFileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                FilePath = filePath,
                ProjectId = projectId,
                UploadedBy = userId,
                UploadedAt = DateTime.UtcNow
            };

            _context.ProjectFiles.Add(projectFile);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                nameof(ProjectFile),
                projectFile.Id,
                "Create",
                userId,
                null,
                $"Uploaded file: {file.FileName}",
                null
            );

            _logger.LogInformation("File uploaded: {FileName} to project {ProjectId}", file.FileName, projectId);
            return Ok(projectFile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, "Error uploading file");
        }
    }

    [HttpGet("{id}/download")]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async Task<IActionResult> DownloadFile(Guid id)
    {
        var file = await _context.ProjectFiles.FindAsync(id);
        if (file == null)
        {
            return NotFound();
        }

        try
        {
            var stream = await _fileStorageService.GetFileAsync(file.FilePath);
            return File(stream, file.ContentType, file.OriginalFileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound("File not found on disk");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file");
            return StatusCode(500, "Error downloading file");
        }
    }

    [HttpGet("project/{projectId}")]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async Task<ActionResult<IEnumerable<ProjectFile>>> GetProjectFiles(Guid projectId)
    {
        var files = await _context.ProjectFiles
            .Where(f => f.ProjectId == projectId)
            .ToListAsync();

        return Ok(files);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "ProjectManagerOrAdmin")]
    public async Task<IActionResult> DeleteFile(Guid id)
    {
        var file = await _context.ProjectFiles.FindAsync(id);
        if (file == null)
        {
            return NotFound();
        }

        var userId = UserContextHelper.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            await _fileStorageService.DeleteFileAsync(file.FilePath);
            _context.ProjectFiles.Remove(file);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                nameof(ProjectFile),
                id,
                "Delete",
                userId,
                null,
                $"Deleted file: {file.OriginalFileName}",
                null
            );

            _logger.LogInformation("File deleted: {FileId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file");
            return StatusCode(500, "Error deleting file");
        }
    }
}

