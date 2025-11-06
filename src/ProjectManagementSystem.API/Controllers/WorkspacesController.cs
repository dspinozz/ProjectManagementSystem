using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.Domain.Entities;
using System.Security.Claims;
using ProjectManagementSystem.Application.Interfaces;

namespace ProjectManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkspacesController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<WorkspacesController> _logger;

    public WorkspacesController(
        IApplicationDbContext context,
        IAuditService auditService,
        ILogger<WorkspacesController> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async Task<ActionResult<IEnumerable<Workspace>>> GetWorkspaces([FromQuery] Guid? organizationId)
    {
        var query = _context.Workspaces
            .Include(w => w.Organization)
            .AsQueryable();

        if (organizationId.HasValue)
        {
            query = query.Where(w => w.OrganizationId == organizationId.Value);
        }

        var workspaces = await query.ToListAsync();
        return Ok(workspaces);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async Task<ActionResult<Workspace>> GetWorkspace(Guid id)
    {
        var workspace = await _context.Workspaces
            .Include(w => w.Organization)
            .Include(w => w.Projects)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workspace == null)
        {
            return NotFound();
        }

        return Ok(workspace);
    }

    [HttpPost]
    [Authorize(Policy = "ProjectManagerOrAdmin")]
    public async Task<ActionResult<Workspace>> CreateWorkspace([FromBody] Workspace workspace)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        workspace.Id = Guid.NewGuid();
        workspace.CreatedAt = DateTime.UtcNow;

        _context.Workspaces.Add(workspace);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            nameof(Workspace),
            workspace.Id,
            "Create",
            userId,
            null,
            $"Created workspace: {workspace.Name}",
            null
        );

        return CreatedAtAction(nameof(GetWorkspace), new { id = workspace.Id }, workspace);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "ProjectManagerOrAdmin")]
    public async Task<IActionResult> UpdateWorkspace(Guid id, [FromBody] Workspace workspace)
    {
        if (id != workspace.Id)
        {
            return BadRequest();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var existingWorkspace = await _context.Workspaces.FindAsync(id);
        if (existingWorkspace == null)
        {
            return NotFound();
        }

        existingWorkspace.Name = workspace.Name;
        existingWorkspace.Description = workspace.Description;
        existingWorkspace.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            nameof(Workspace),
            id,
            "Update",
            userId,
            null,
            $"Updated workspace: {workspace.Name}",
            null
        );

        return Ok(existingWorkspace);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteWorkspace(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var workspace = await _context.Workspaces.FindAsync(id);
        if (workspace == null)
        {
            return NotFound();
        }

        _context.Workspaces.Remove(workspace);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            nameof(Workspace),
            id,
            "Delete",
            userId,
            null,
            $"Deleted workspace: {workspace.Name}",
            null
        );

        return NoContent();
    }
}

