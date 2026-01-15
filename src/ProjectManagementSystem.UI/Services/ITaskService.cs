using System.Net;
using System.Net.Http.Json;
using ProjectManagementSystem.UI.Models;

namespace ProjectManagementSystem.UI.Services;

public interface ITaskService
{
    Task<List<TaskDto>> GetTasksAsync(string? projectId);
    Task<List<TaskDto>> GetTasksByProjectAsync(string projectId);
    Task<TaskDto?> GetTaskByIdAsync(string id);
    Task<bool> CreateTaskAsync(CreateTaskRequest request);
    Task<bool> UpdateTaskAsync(string id, UpdateTaskRequest request);
    Task<bool> DeleteTaskAsync(string id);
}

public class TaskService : ITaskService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public TaskService(IHttpClientFactory httpClientFactory, IAuthService authService)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        _authService = authService;
    }

    private async Task<HttpRequestMessage> CreateAuthenticatedRequestAsync(HttpMethod method, string uri)
    {
        var request = new HttpRequestMessage(method, uri);
        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        return request;
    }

    public async Task<List<TaskDto>> GetTasksAsync(string? projectId)
    {
        var uri = projectId != null ? $"/api/tasks?projectId={projectId}" : "/api/tasks";
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Get, uri);
        var response = await _httpClient.SendAsync(httpRequest);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<TaskDto>>() ?? new();
        
        throw new HttpRequestException($"Failed to load tasks: {response.StatusCode}");
    }

    public async Task<List<TaskDto>> GetTasksByProjectAsync(string projectId)
    {
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Get, $"/api/tasks/project/{projectId}");
        var response = await _httpClient.SendAsync(httpRequest);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<TaskDto>>() ?? new();
        
        throw new HttpRequestException($"Failed to load tasks: {response.StatusCode}");
    }

    public async Task<TaskDto?> GetTaskByIdAsync(string id)
    {
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Get, $"/api/tasks/{id}");
        var response = await _httpClient.SendAsync(httpRequest);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<TaskDto>();
        
        throw new HttpRequestException($"Failed to load task: {response.StatusCode}");
    }

    public async Task<bool> CreateTaskAsync(CreateTaskRequest request)
    {
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Post, "/api/tasks");
        httpRequest.Content = JsonContent.Create(request);
        var response = await _httpClient.SendAsync(httpRequest);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new ArgumentException($"Validation failed: {error}");
        }
        
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateTaskAsync(string id, UpdateTaskRequest request)
    {
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Put, $"/api/tasks/{id}");
        httpRequest.Content = JsonContent.Create(request);
        var response = await _httpClient.SendAsync(httpRequest);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteTaskAsync(string id)
    {
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Delete, $"/api/tasks/{id}");
        var response = await _httpClient.SendAsync(httpRequest);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        
        return response.IsSuccessStatusCode;
    }
}
