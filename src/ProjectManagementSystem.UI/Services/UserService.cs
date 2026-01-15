using System.Net;
using System.Net.Http.Json;
using ProjectManagementSystem.UI.Models;

namespace ProjectManagementSystem.UI.Services;

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public UserService(IHttpClientFactory httpClientFactory, IAuthService authService)
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

    public async Task<List<UserSearchDto>> SearchUsersAsync(string? search = null, string? organizationId = null, string? workspaceId = null, int page = 1, int pageSize = 50)
    {
        try
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(search))
            {
                queryParams.Add($"search={Uri.EscapeDataString(search)}");
            }
            if (!string.IsNullOrWhiteSpace(organizationId))
            {
                queryParams.Add($"organizationId={Uri.EscapeDataString(organizationId)}");
            }
            if (!string.IsNullOrWhiteSpace(workspaceId))
            {
                queryParams.Add($"workspaceId={Uri.EscapeDataString(workspaceId)}");
            }
            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Get, $"/api/users{queryString}");
            var response = await _httpClient.SendAsync(httpRequest);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Session expired. Please login again.");
            }
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<UserSearchDto>>() ?? new();
            }
            
            throw new HttpRequestException($"Failed to search users: {response.StatusCode}");
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error searching users: {ex.Message}", ex);
        }
    }

    public async Task<UserSearchDto?> GetUserByIdAsync(string id)
    {
        try
        {
            var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Get, $"/api/users/{id}");
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
                return await response.Content.ReadFromJsonAsync<UserSearchDto>();
            }
            
            throw new HttpRequestException($"Failed to load user: {response.StatusCode}");
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading user: {ex.Message}", ex);
        }
    }

    public async Task<UserSearchDto?> GetCurrentUserAsync()
    {
        try
        {
            var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Get, "/api/users/me");
            var response = await _httpClient.SendAsync(httpRequest);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Session expired. Please login again.");
            }
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserSearchDto>();
            }
            
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading current user: {ex.Message}", ex);
        }
    }
}
