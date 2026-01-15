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
public class OrganizationsController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<OrganizationsController> _logger;

    public OrganizationsController(
        IApplicationDbContext context,
        IAuditService auditService,
        ILogger<OrganizationsController> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async Task<ActionResult<IEnumerable<object>>> GetOrganizations()
    {
        var organizations = await _context.Organizations
            .Select(o => new {
                o.Id,
                o.Name,
                o.Description,
                o.CreatedAt,
                o.UpdatedAt,
                WorkspaceCount = o.Workspaces.Count
            }).ToListAsync();
        return Ok(organizations);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async Task<ActionResult<Organization>> GetOrganization(Guid id)
    {
        var organization = await _context.Organizations
            .Include(o => o.Workspaces)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (organization == null)
        {
            return NotFound();
        }

        return Ok(organization);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<Organization>> CreateOrganization([FromBody] Organization organization)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        organization.Id = Guid.NewGuid();
        organization.CreatedAt = DateTime.UtcNow;

        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            nameof(Organization),
            organization.Id,
            "Create",
            userId,
            null,
            $"Created organization: {organization.Name}",
            null
        );

        return CreatedAtAction(nameof(GetOrganization), new { id = organization.Id }, organization);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateOrganization(Guid id, [FromBody] Organization organization)
    {
        if (id != organization.Id)
        {
            return BadRequest();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var existingOrg = await _context.Organizations.FindAsync(id);
        if (existingOrg == null)
        {
            return NotFound();
        }

        existingOrg.Name = organization.Name;
        existingOrg.Description = organization.Description;
        existingOrg.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            nameof(Organization),
            id,
            "Update",
            userId,
            null,
            $"Updated organization: {organization.Name}",
            null
        );

        return Ok(existingOrg);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteOrganization(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var organization = await _context.Organizations.FindAsync(id);
        if (organization == null)
        {
            return NotFound();
        }

        _context.Organizations.Remove(organization);
        await _context.SaveChangesAsync();

        await _auditService.LogAsync(
            nameof(Organization),
            id,
            "Delete",
            userId,
            null,
            $"Deleted organization: {organization.Name}",
            null
        );

        return NoContent();
    }
}

