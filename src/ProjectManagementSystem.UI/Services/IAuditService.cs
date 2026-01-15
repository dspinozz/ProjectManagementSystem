using System.Net;
using System.Net.Http.Json;
using ProjectManagementSystem.UI.Models;

namespace ProjectManagementSystem.UI.Services;

public interface IAuditService
{
    Task<List<AuditLogDto>> GetAuditLogsAsync(int page = 1, int pageSize = 50);
}

public class AuditService : IAuditService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public AuditService(IHttpClientFactory httpClientFactory, IAuthService authService)
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

    public async Task<List<AuditLogDto>> GetAuditLogsAsync(int page = 1, int pageSize = 50)
    {
        try
        {
            var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Get, $"/api/audit?page={page}&pageSize={pageSize}");
            var response = await _httpClient.SendAsync(httpRequest);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<AuditLogDto>>() ?? new();
            }
        }
        catch { }
        return new();
    }
}
