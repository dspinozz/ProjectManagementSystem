using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementSystem.API.DTOs;
using ProjectManagementSystem.Domain.Entities;
using System.Security.Claims;

namespace ProjectManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        ILogger<UsersController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async Task<ActionResult<IEnumerable<UserSearchResponseDto>>> GetUsers(
        [FromQuery] string? search = null,
        [FromQuery] Guid? organizationId = null,
        [FromQuery] Guid? workspaceId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var query = _userManager.Users.AsQueryable();

            // Filter by organization if provided
            if (organizationId.HasValue)
            {
                query = query.Where(u => u.OrganizationId == organizationId.Value);
            }

            // Filter by workspace if provided
            if (workspaceId.HasValue)
            {
                query = query.Where(u => u.WorkspaceId == workspaceId.Value);
            }

            // Search by email, first name, or last name
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(u =>
                    u.Email!.ToLower().Contains(searchLower) ||
                    u.FirstName.ToLower().Contains(searchLower) ||
                    u.LastName.ToLower().Contains(searchLower));
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var users = await query
                .OrderBy(u => u.Email)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserSearchResponseDto
                {
                    Id = u.Id,
                    Email = u.Email ?? string.Empty,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    OrganizationId = u.OrganizationId.HasValue ? u.OrganizationId.Value.ToString() : null,
                    WorkspaceId = u.WorkspaceId.HasValue ? u.WorkspaceId.Value.ToString() : null
                })
                .ToListAsync();

            Response.Headers["X-Total-Count"] = totalCount.ToString();
            Response.Headers["X-Page"] = page.ToString();
            Response.Headers["X-Page-Size"] = pageSize.ToString();

            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, "An error occurred while retrieving users");
        }
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async Task<ActionResult<UserSearchResponseDto>> GetUser(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var dto = new UserSearchResponseDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                OrganizationId = user.OrganizationId.HasValue ? user.OrganizationId.Value.ToString() : null,
                WorkspaceId = user.WorkspaceId.HasValue ? user.WorkspaceId.Value.ToString() : null
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user");
            return StatusCode(500, "An error occurred while retrieving the user");
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserSearchResponseDto>> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        return await GetUser(userId);
    }
}
