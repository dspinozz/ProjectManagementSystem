using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Application.Services;
using ProjectManagementSystem.Domain.Entities;
using ProjectManagementSystem.API.DTOs;
using ProjectManagementSystem.API.Mappings;
using System.Security.Claims;

namespace ProjectManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async Task<ActionResult<IEnumerable<ProjectResponseDto>>> GetProjects([FromQuery] Guid? workspaceId)
    {
        var projects = await _projectService.GetAllAsync(workspaceId);
        var dtos = projects.Select(p => p.ToDto());
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async Task<ActionResult<ProjectResponseDto>> GetProject(Guid id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }
        return Ok(project.ToDto());
    }

    [HttpPost]
    [Authorize(Policy = "ProjectManagerOrAdmin")]
    public async Task<ActionResult<ProjectResponseDto>> CreateProject([FromBody] CreateProjectRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var project = request.ToEntity();
            var createdProject = await _projectService.CreateAsync(project, userId);
            return CreatedAtAction(nameof(GetProject), new { id = createdProject.Id }, createdProject.ToDto());
        }
        catch (FormatException)
        {
            return BadRequest("Invalid Workspace ID format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return StatusCode(500, "An error occurred while creating the project");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "ProjectManagerOrAdmin")]
    public async Task<ActionResult<ProjectResponseDto>> UpdateProject(Guid id, [FromBody] UpdateProjectRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var existingProject = await _projectService.GetByIdAsync(id);
            if (existingProject == null)
            {
                return NotFound();
            }

            existingProject.UpdateEntity(request);
            var updatedProject = await _projectService.UpdateAsync(existingProject, userId);
            return Ok(updatedProject.ToDto());
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project");
            return StatusCode(500, "An error occurred while updating the project");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await _projectService.DeleteAsync(id, userId);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}

