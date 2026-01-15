using System.Net;
using System.Net.Http.Json;
using ProjectManagementSystem.UI.Models;

namespace ProjectManagementSystem.UI.Services;

public class MemberService : IMemberService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public MemberService(IHttpClientFactory httpClientFactory, IAuthService authService)
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

    public async Task<List<MemberDto>> GetProjectMembersAsync(string projectId)
    {
        try
        {
            var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Get, $"/api/projects/{projectId}/members");
            var response = await _httpClient.SendAsync(httpRequest);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Session expired. Please login again.");
            }
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<MemberDto>>() ?? new();
            }
            
            throw new HttpRequestException($"Failed to load project members: {response.StatusCode}");
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading project members: {ex.Message}", ex);
        }
    }

    public async Task<MemberDto> AddMemberAsync(string projectId, AddMemberRequest request)
    {
        try
        {
            var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Post, $"/api/projects/{projectId}/members");
            httpRequest.Content = JsonContent.Create(request);
            var response = await _httpClient.SendAsync(httpRequest);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Session expired. Please login again.");
            }
            
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("You don't have permission to add members to this project.");
            }
            
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ArgumentException($"Validation failed: {errorContent}");
            }
            
            if (response.IsSuccessStatusCode)
            {
                var member = await response.Content.ReadFromJsonAsync<MemberDto>();
                if (member == null)
                {
                    throw new Exception("Failed to parse member response");
                }
                return member;
            }
            
            throw new HttpRequestException($"Failed to add member: {response.StatusCode}");
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
            throw new Exception($"Error adding member: {ex.Message}", ex);
        }
    }

    public async Task<MemberDto> UpdateMemberRoleAsync(string projectId, string userId, UpdateMemberRoleRequest request)
    {
        try
        {
            var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Put, $"/api/projects/{projectId}/members/{userId}");
            httpRequest.Content = JsonContent.Create(request);
            var response = await _httpClient.SendAsync(httpRequest);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Session expired. Please login again.");
            }
            
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("You don't have permission to update member roles in this project.");
            }
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Member not found.");
            }
            
            if (response.IsSuccessStatusCode)
            {
                var member = await response.Content.ReadFromJsonAsync<MemberDto>();
                if (member == null)
                {
                    throw new Exception("Failed to parse member response");
                }
                return member;
            }
            
            throw new HttpRequestException($"Failed to update member role: {response.StatusCode}");
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
            throw new Exception($"Error updating member role: {ex.Message}", ex);
        }
    }

    public async Task<bool> RemoveMemberAsync(string projectId, string userId)
    {
        try
        {
            var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Delete, $"/api/projects/{projectId}/members/{userId}");
            var response = await _httpClient.SendAsync(httpRequest);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Session expired. Please login again.");
            }
            
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("You don't have permission to remove members from this project.");
            }
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("Member not found.");
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
            throw new Exception($"Error removing member: {ex.Message}", ex);
        }
    }
}
