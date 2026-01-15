using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using ProjectManagementSystem.UI.Models;

namespace ProjectManagementSystem.UI.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<AuthService> _logger;
    private const string TokenKey = "authToken";
    private const string UserKey = "userData";

    public AuthService(IHttpClientFactory httpClientFactory, ILocalStorageService localStorage, ILogger<AuthService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        _localStorage = localStorage;
        _logger = logger;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            _logger.LogInformation("Attempting login for {Email} to {BaseAddress}", email, _httpClient.BaseAddress);
            
            var request = new LoginRequest { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);

            _logger.LogInformation("Login response status: {StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Login response content: {Content}", content.Substring(0, Math.Min(200, content.Length)));
                
                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(content);
                
                // API returns camelCase: "token" and "user"
                if (jsonResponse.TryGetProperty("token", out var tokenElement) && 
                    jsonResponse.TryGetProperty("user", out var userElement))
                {
                    var token = tokenElement.GetString();
                    var user = userElement.Deserialize<UserDto>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    _logger.LogInformation("Token extracted: {HasToken}, User extracted: {HasUser}", !string.IsNullOrEmpty(token), user != null);
                    
                    if (!string.IsNullOrEmpty(token) && user != null)
                    {
                        // Extract role from JWT token
                        var role = JwtHelper.GetRoleFromToken(token);
                        user.Role = role ?? "TeamMember"; // Default to TeamMember if role not found
                        
                        await _localStorage.SetItemAsync(TokenKey, token);
                        await _localStorage.SetItemAsync(UserKey, user);
                        _logger.LogInformation("Login successful for {Email}", email);
                        return true;
                    }
                }
                else
                {
                    _logger.LogWarning("Could not find token/user in response");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Login failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login exception for {Email}", email);
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
        await _localStorage.RemoveItemAsync(UserKey);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>(TokenKey);
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetUserRoleAsync()
    {
        var user = await _localStorage.GetItemAsync<UserDto>(UserKey);
        return user?.Role;
    }

    public async Task<UserDto?> GetUserAsync()
    {
        return await _localStorage.GetItemAsync<UserDto>(UserKey);
    }
}

// Local storage service interface
public interface ILocalStorageService
{
    Task<T?> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T value);
    Task RemoveItemAsync(string key);
}

// In-memory implementation for Blazor Server
// Uses scoped service to maintain per-connection storage in Blazor Server SignalR context
public class InMemoryLocalStorageService : ILocalStorageService
{
    private readonly Dictionary<string, object> _storage = new();

    public Task<T?> GetItemAsync<T>(string key)
    {
        if (_storage.TryGetValue(key, out var value))
        {
            return Task.FromResult((T?)value);
        }
        return Task.FromResult<T?>(default);
    }

    public Task SetItemAsync<T>(string key, T value)
    {
        _storage[key] = value!;
        return Task.CompletedTask;
    }

    public Task RemoveItemAsync(string key)
    {
        _storage.Remove(key);
        return Task.CompletedTask;
    }
}
