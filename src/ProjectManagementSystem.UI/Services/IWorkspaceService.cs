using System.Net;
using System.Net.Http.Json;
using ProjectManagementSystem.UI.Models;

namespace ProjectManagementSystem.UI.Services;

public interface IWorkspaceService
{
    Task<List<WorkspaceDto>> GetWorkspacesAsync(string? organizationId = null);
    Task<WorkspaceDto?> GetWorkspaceByIdAsync(string id);
    Task<bool> CreateWorkspaceAsync(CreateWorkspaceRequest request);
    Task<bool> UpdateWorkspaceAsync(string id, UpdateWorkspaceRequest request);
    Task<bool> DeleteWorkspaceAsync(string id);
}

public class WorkspaceService : IWorkspaceService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public WorkspaceService(IHttpClientFactory httpClientFactory, IAuthService authService)
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

    public async Task<List<WorkspaceDto>> GetWorkspacesAsync(string? organizationId = null)
    {
        var uri = organizationId != null ? $"/api/workspaces?organizationId={organizationId}" : "/api/workspaces";
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Get, uri);
        var response = await _httpClient.SendAsync(httpRequest);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<WorkspaceDto>>() ?? new();
        
        throw new HttpRequestException($"Failed to load workspaces: {response.StatusCode}");
    }

    public async Task<WorkspaceDto?> GetWorkspaceByIdAsync(string id)
    {
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Get, $"/api/workspaces/{id}");
        var response = await _httpClient.SendAsync(httpRequest);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<WorkspaceDto>();
        
        throw new HttpRequestException($"Failed to load workspace: {response.StatusCode}");
    }

    public async Task<bool> CreateWorkspaceAsync(CreateWorkspaceRequest request)
    {
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Post, "/api/workspaces");
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

    public async Task<bool> UpdateWorkspaceAsync(string id, UpdateWorkspaceRequest request)
    {
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Put, $"/api/workspaces/{id}");
        httpRequest.Content = JsonContent.Create(request);
        var response = await _httpClient.SendAsync(httpRequest);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteWorkspaceAsync(string id)
    {
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Delete, $"/api/workspaces/{id}");
        var response = await _httpClient.SendAsync(httpRequest);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        
        return response.IsSuccessStatusCode;
    }
}
