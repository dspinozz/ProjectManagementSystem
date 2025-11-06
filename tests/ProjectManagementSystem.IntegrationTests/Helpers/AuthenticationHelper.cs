using System.Net.Http.Json;
using System.Text.Json;

namespace ProjectManagementSystem.IntegrationTests.Helpers;

/// <summary>
/// Helper class for authentication in integration tests.
/// Uses seeded test users from TestDataSeeder for consistent, reliable authentication.
/// </summary>
public static class AuthenticationHelper
{
    /// <summary>
    /// Gets an authentication token using the seeded test user.
    /// This is the recommended approach for integration tests.
    /// </summary>
    public static async System.Threading.Tasks.Task<string> GetAuthTokenAsync(HttpClient client)
    {
        // Use the seeded test user credentials
        var loginRequest = new
        {
            Email = "testuser@example.com",
            Password = "Test1234!"
        };

        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to get auth token. Status: {response.StatusCode}, Content: {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        if (!json.RootElement.TryGetProperty("token", out var tokenElement))
        {
            // Try capitalized "Token" as fallback
            if (!json.RootElement.TryGetProperty("Token", out tokenElement))
            {
                throw new InvalidOperationException(
                    $"Token not found in response. Response: {content}");
            }
        }

        var token = tokenElement.GetString();
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("Token is null or empty in response.");
        }

        return token;
    }

    /// <summary>
    /// Gets an authentication token for the admin user.
    /// </summary>
    public static async System.Threading.Tasks.Task<string> GetAdminAuthTokenAsync(HttpClient client)
    {
        var loginRequest = new
        {
            Email = "admin@example.com",
            Password = "Admin1234!"
        };

        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Failed to get admin auth token. Status: {response.StatusCode}, Content: {errorContent}");
        }

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        
        if (!json.RootElement.TryGetProperty("token", out var tokenElement))
        {
            if (!json.RootElement.TryGetProperty("Token", out tokenElement))
            {
                throw new InvalidOperationException(
                    $"Token not found in response. Response: {content}");
            }
        }

        var token = tokenElement.GetString();
        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("Token is null or empty in response.");
        }

        return token;
    }

    /// <summary>
    /// Creates an HttpClient with authentication header set.
    /// </summary>
    public static HttpClient CreateAuthenticatedClient(HttpClient baseClient, string token)
    {
        var authenticatedClient = new HttpClient
        {
            BaseAddress = baseClient.BaseAddress
        };
        authenticatedClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        return authenticatedClient;
    }
}

