using System.Net;
using System.Net.Http.Json;
using ProjectManagementSystem.UI.Models;

namespace ProjectManagementSystem.UI.Services;

public interface IOrganizationService
{
    Task<List<OrganizationDto>> GetOrganizationsAsync();
    Task<OrganizationDto?> GetOrganizationByIdAsync(string id);
    Task<bool> CreateOrganizationAsync(CreateOrganizationRequest request);
    Task<bool> UpdateOrganizationAsync(string id, UpdateOrganizationRequest request);
    Task<bool> DeleteOrganizationAsync(string id);
}

public class OrganizationService : IOrganizationService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public OrganizationService(IHttpClientFactory httpClientFactory, IAuthService authService)
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

    public async Task<List<OrganizationDto>> GetOrganizationsAsync()
    {
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Get, "/api/organizations");
        var response = await _httpClient.SendAsync(httpRequest);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<OrganizationDto>>() ?? new();
        
        throw new HttpRequestException($"Failed to load organizations: {response.StatusCode}");
    }

    public async Task<OrganizationDto?> GetOrganizationByIdAsync(string id)
    {
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Get, $"/api/organizations/{id}");
        var response = await _httpClient.SendAsync(httpRequest);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<OrganizationDto>();
        
        throw new HttpRequestException($"Failed to load organization: {response.StatusCode}");
    }

    public async Task<bool> CreateOrganizationAsync(CreateOrganizationRequest request)
    {
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Post, "/api/organizations");
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

    public async Task<bool> UpdateOrganizationAsync(string id, UpdateOrganizationRequest request)
    {
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Put, $"/api/organizations/{id}");
        httpRequest.Content = JsonContent.Create(request);
        var response = await _httpClient.SendAsync(httpRequest);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteOrganizationAsync(string id)
    {
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Delete, $"/api/organizations/{id}");
        var response = await _httpClient.SendAsync(httpRequest);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        
        return response.IsSuccessStatusCode;
    }
}
