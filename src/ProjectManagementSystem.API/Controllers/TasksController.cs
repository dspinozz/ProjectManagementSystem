using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementSystem.Application.Services;
using ProjectManagementSystem.Domain.Entities;
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

    [HttpGet("project/{projectId}")]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async System.Threading.Tasks.Task<ActionResult<IEnumerable<TaskEntity>>> GetTasksByProject(Guid projectId)
    {
        var tasks = await _taskService.GetByProjectIdAsync(projectId);
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async System.Threading.Tasks.Task<ActionResult<TaskEntity>> GetTask(Guid id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }
        return Ok(task);
    }

    [HttpPost]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async System.Threading.Tasks.Task<ActionResult<TaskEntity>> CreateTask([FromBody] TaskEntity task)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            var createdTask = await _taskService.CreateAsync(task, userId);
            return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "TeamMemberOrAbove")]
    public async System.Threading.Tasks.Task<IActionResult> UpdateTask(Guid id, [FromBody] TaskEntity task)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        if (id != task.Id)
        {
            return BadRequest("Task ID mismatch");
        }

        try
        {
            var updatedTask = await _taskService.UpdateAsync(task, userId);
            return Ok(updatedTask);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
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
