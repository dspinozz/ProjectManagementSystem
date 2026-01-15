using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Application.Services;
using ProjectManagementSystem.Domain.Entities;
using ProjectManagementSystem.API.DTOs;
using ProjectManagementSystem.API.Mappings;
using TaskEntity = ProjectManagementSystem.Domain.Entities.Task;
using System.Security.Claims;

namespace ProjectManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasks([FromQuery] Guid? projectId)
    {
        var tasks = await _taskService.GetAllAsync(projectId);
        var dtos = tasks.Select(t => t.ToDto());
        return Ok(dtos);
    }

    [HttpGet("project/{projectId}")]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasksByProject(Guid projectId)
    {
        var tasks = await _taskService.GetByProjectIdAsync(projectId);
        var dtos = tasks.Select(t => t.ToDto());
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async Task<ActionResult<TaskResponseDto>> GetTask(Guid id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }
        return Ok(task.ToDto());
    }

    [HttpPost]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async Task<ActionResult<TaskResponseDto>> CreateTask([FromBody] CreateTaskRequestDto request)
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
            var task = request.ToEntity();
            var createdTask = await _taskService.CreateAsync(task, userId);
            return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask.ToDto());
        }
        catch (FormatException)
        {
            return BadRequest("Invalid Project ID format");
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, "An error occurred while creating the task");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async Task<ActionResult<TaskResponseDto>> UpdateTask(Guid id, [FromBody] UpdateTaskRequestDto request)
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
            var existingTask = await _taskService.GetByIdAsync(id);
            if (existingTask == null)
            {
                return NotFound();
            }

            existingTask.UpdateEntity(request);
            var updatedTask = await _taskService.UpdateAsync(existingTask, userId);
            return Ok(updatedTask.ToDto());
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task");
            return StatusCode(500, "An error occurred while updating the task");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "ProjectManagerOrAdmin")]
    public async System.Threading.Tasks.Task<IActionResult> DeleteTask(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var deleted = await _taskService.DeleteAsync(id, userId);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
