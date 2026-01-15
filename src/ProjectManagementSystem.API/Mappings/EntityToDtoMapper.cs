using ProjectManagementSystem.API.DTOs;
using ProjectManagementSystem.Domain.Entities;
using TaskEntity = ProjectManagementSystem.Domain.Entities.Task;
using DomainTaskStatus = ProjectManagementSystem.Domain.Entities.TaskStatus;
using DomainTaskPriority = ProjectManagementSystem.Domain.Entities.TaskPriority;

namespace ProjectManagementSystem.API.Mappings;

public static class EntityToDtoMapper
{
    public static ProjectResponseDto ToDto(this Project project)
    {
        return new ProjectResponseDto
        {
            Id = project.Id.ToString(),
            Name = project.Name,
            Description = project.Description,
            Status = (int)project.Status,
            WorkspaceId = project.WorkspaceId.ToString(),
            CreatedBy = project.CreatedBy,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            StartDate = project.StartDate,
            EndDate = project.EndDate
        };
    }

    public static TaskResponseDto ToDto(this TaskEntity task)
    {
        return new TaskResponseDto
        {
            Id = task.Id.ToString(),
            Title = task.Title,
            Description = task.Description,
            Status = (int)task.Status,
            Priority = (int)task.Priority,
            ProjectId = task.ProjectId.ToString(),
            AssignedToId = task.AssignedToId,
            CreatedBy = task.CreatedBy,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            DueDate = task.DueDate
        };
    }

    public static Project ToEntity(this CreateProjectRequestDto dto, Guid? id = null)
    {
        return new Project
        {
            Id = id ?? Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Status = (ProjectStatus)dto.Status,
            WorkspaceId = Guid.Parse(dto.WorkspaceId),
            StartDate = dto.StartDate,
            EndDate = dto.EndDate
        };
    }

    public static void UpdateEntity(this Project entity, UpdateProjectRequestDto dto)
    {
        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Status = (ProjectStatus)dto.Status;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
    }

    public static TaskEntity ToEntity(this CreateTaskRequestDto dto, Guid? id = null)
    {
        return new TaskEntity
        {
            Id = id ?? Guid.NewGuid(),
            Title = dto.Title,
            Description = dto.Description,
            Status = (DomainTaskStatus)dto.Status,
            Priority = (DomainTaskPriority)dto.Priority,
            ProjectId = Guid.Parse(dto.ProjectId),
            AssignedToId = dto.AssignedToId,
            DueDate = dto.DueDate
        };
    }

    public static void UpdateEntity(this TaskEntity entity, UpdateTaskRequestDto dto)
    {
        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.Status = (DomainTaskStatus)dto.Status;
        entity.Priority = (DomainTaskPriority)dto.Priority;
        entity.AssignedToId = dto.AssignedToId;
        entity.DueDate = dto.DueDate;
    }
}
