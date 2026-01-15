using System.Net;
using System.Net.Http.Json;
using ProjectManagementSystem.UI.Models;

namespace ProjectManagementSystem.UI.Services;

public interface IProjectService
{
    Task<List<ProjectDto>> GetProjectsAsync();
    Task<ProjectDto?> GetProjectByIdAsync(string id);
    Task<bool> CreateProjectAsync(CreateProjectRequest request);
    Task<bool> UpdateProjectAsync(string id, UpdateProjectRequest request);
    Task<bool> DeleteProjectAsync(string id);
}

public class ProjectService : IProjectService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public ProjectService(IHttpClientFactory httpClientFactory, IAuthService authService)
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
            request.Headers.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        return request;
    }

    public async Task<List<ProjectDto>> GetProjectsAsync()
    {
        try
        {
            var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Get, "/api/projects");
            var response = await _httpClient.SendAsync(httpRequest);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Session expired. Please login again.");
            }
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<ProjectDto>>() ?? new();
            }
            
            throw new HttpRequestException($"Failed to load projects: {response.StatusCode}");
        }
        catch (UnauthorizedAccessException)
        {
            throw; // Re-throw to handle in UI
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw to handle in UI
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading projects: {ex.Message}", ex);
        }
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(string id)
    {
        try
        {
            var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Get, $"/api/projects/{id}");
            var response = await _httpClient.SendAsync(httpRequest);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Session expired. Please login again.");
            }
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProjectDto>();
            }
            
            throw new HttpRequestException($"Failed to load project: {response.StatusCode}");
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading project: {ex.Message}", ex);
        }
    }

    public async Task<bool> CreateProjectAsync(CreateProjectRequest request)
    {
        try
        {
            var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Post, "/api/projects");
            httpRequest.Content = JsonContent.Create(request);
            var response = await _httpClient.SendAsync(httpRequest);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Session expired. Please login again.");
            }
            
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ArgumentException($"Validation failed: {errorContent}");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error creating project: {ex.Message}", ex);
        }
    }

    public async Task<bool> UpdateProjectAsync(string id, UpdateProjectRequest request)
    {
        try
        {
            var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Put, $"/api/projects/{id}");
            httpRequest.Content = JsonContent.Create(request);
            var response = await _httpClient.SendAsync(httpRequest);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Session expired. Please login again.");
            }
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Project not found.");
            }
            
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ArgumentException($"Validation failed: {errorContent}");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating project: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteProjectAsync(string id)
    {
        try
        {
            var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Delete, $"/api/projects/{id}");
            var response = await _httpClient.SendAsync(httpRequest);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Session expired. Please login again.");
            }
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Project not found.");
            }
            
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("You don't have permission to delete this project.");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting project: {ex.Message}", ex);
        }
    }
}
