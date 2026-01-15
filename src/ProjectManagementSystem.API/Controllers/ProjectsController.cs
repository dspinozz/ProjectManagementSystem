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

    [HttpGet("{projectId}/members")]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async Task<ActionResult<IEnumerable<MemberResponseDto>>> GetProjectMembers(Guid projectId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Verify project exists
        var project = await _projectService.GetByIdAsync(projectId);
        if (project == null)
        {
            return NotFound("Project not found");
        }

        // Check if user is a member of the project
        var isMember = await _projectService.IsUserProjectMemberAsync(projectId, userId);
        if (!isMember && !User.IsInRole("Admin"))
        {
            return Forbid("You must be a member of this project to view its members");
        }

        try
        {
            var members = await _projectService.GetProjectMembersAsync(projectId);
            var dtos = members.Select(m => m.ToDto());
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project members");
            return StatusCode(500, "An error occurred while retrieving project members");
        }
    }

    [HttpPost("{projectId}/members")]
    [Authorize(Policy = "ProjectManagerOrAdmin")]
    public async Task<ActionResult<MemberResponseDto>> AddProjectMember(Guid projectId, [FromBody] AddMemberRequestDto request)
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

        // Verify requester is a project member (or admin)
        var requesterRole = await _projectService.GetUserProjectRoleAsync(projectId, userId);
        if (requesterRole == null && !User.IsInRole("Admin"))
        {
            return Forbid("You must be a member of this project to add members");
        }

        // Only ProjectManager or Admin can add members
        if (requesterRole != ProjectRole.ProjectManager && !User.IsInRole("Admin"))
        {
            return Forbid("Only project managers and admins can add members");
        }

        try
        {
            var role = (ProjectRole)request.Role;
            var member = await _projectService.AddMemberAsync(projectId, request.UserId, role, userId);
            return CreatedAtAction(nameof(GetProjectMembers), new { projectId }, member.ToDto());
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding project member");
            return StatusCode(500, "An error occurred while adding the project member");
        }
    }

    [HttpPut("{projectId}/members/{memberUserId}")]
    [Authorize(Policy = "ProjectManagerOrAdmin")]
    public async Task<ActionResult<MemberResponseDto>> UpdateProjectMemberRole(
        Guid projectId, 
        string memberUserId, 
        [FromBody] UpdateMemberRoleRequestDto request)
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

        // Verify requester is a project manager or admin
        var requesterRole = await _projectService.GetUserProjectRoleAsync(projectId, userId);
        if (requesterRole != ProjectRole.ProjectManager && !User.IsInRole("Admin"))
        {
            return Forbid("Only project managers and admins can update member roles");
        }

        try
        {
            var newRole = (ProjectRole)request.Role;
            var member = await _projectService.UpdateMemberRoleAsync(projectId, memberUserId, newRole, userId);
            return Ok(member.ToDto());
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project member role");
            return StatusCode(500, "An error occurred while updating the project member role");
        }
    }

    [HttpDelete("{projectId}/members/{memberUserId}")]
    [Authorize(Policy = "ProjectManagerOrAdmin")]
    public async Task<IActionResult> RemoveProjectMember(Guid projectId, string memberUserId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        // Verify requester is a project manager or admin
        var requesterRole = await _projectService.GetUserProjectRoleAsync(projectId, userId);
        if (requesterRole != ProjectRole.ProjectManager && !User.IsInRole("Admin"))
        {
            return Forbid("Only project managers and admins can remove members");
        }

        // Prevent removing the last project manager
        var memberToRemoveRole = await _projectService.GetUserProjectRoleAsync(projectId, memberUserId);
        if (memberToRemoveRole == ProjectRole.ProjectManager)
        {
            var allMembers = await _projectService.GetProjectMembersAsync(projectId);
            var projectManagerCount = allMembers.Count(m => m.Role == ProjectRole.ProjectManager);
            if (projectManagerCount <= 1)
            {
                return BadRequest("Cannot remove the last project manager from a project");
            }
        }

        try
        {
            var result = await _projectService.RemoveMemberAsync(projectId, memberUserId, userId);
            if (!result)
            {
                return NotFound("Member not found in this project");
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing project member");
            return StatusCode(500, "An error occurred while removing the project member");
        }
    }
}

