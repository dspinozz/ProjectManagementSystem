# Authentication Best Practices for Integration Tests

## Overview
This document explains the best practices for handling authentication in integration tests.

## ✅ Best Practice: Use Seeded Test Users

**DO:**
- Use the seeded test users from `TestDataSeeder` (testuser@example.com / Test1234!)
- Use the `AuthenticationHelper` class to get tokens
- Tokens are generated fresh for each test run (realistic behavior)
- No static tokens (they expire and aren't realistic)

**DON'T:**
- ❌ Don't use static/hardcoded tokens
- ❌ Don't register new users in every test (inefficient)
- ❌ Don't use `.Result` on async methods without proper error handling

## Implementation

### 1. AuthenticationHelper Class
Located at: `tests/ProjectManagementSystem.IntegrationTests/Helpers/AuthenticationHelper.cs`

This helper provides:
- `GetAuthTokenAsync(HttpClient)` - Gets token for regular test user
- `GetAdminAuthTokenAsync(HttpClient)` - Gets token for admin user
- `CreateAuthenticatedClient(HttpClient, string)` - Creates authenticated client

### 2. Usage in Test Classes

```csharp
public class MyControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly string _authToken;

    public MyControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        // Get token using seeded test user
        _authToken = AuthenticationHelper.GetAuthTokenAsync(_client).GetAwaiter().GetResult();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");
    }
}
```

### 3. Why This Approach?

1. **Reliability**: Seeded users are guaranteed to exist
2. **Performance**: No need to register users for each test
3. **Realistic**: Uses actual authentication flow (not mocked)
4. **Maintainable**: Single source of truth for test credentials
5. **Flexible**: Can easily switch between regular and admin users

## Alternative Approaches (Not Recommended)

### ❌ Static Tokens
```csharp
// DON'T DO THIS
private const string STATIC_TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
```
**Problems:**
- Tokens expire
- Not realistic (bypasses actual auth flow)
- Security risk if committed

### ❌ Register User Per Test
```csharp
// DON'T DO THIS
private async Task<string> GetToken()
{
    var email = $"test{Guid.NewGuid()}@example.com";
    await RegisterUser(email);
    return await Login(email);
}
```
**Problems:**
- Slower (extra HTTP calls)
- More complex
- Unnecessary database operations

## Troubleshooting

### Issue: "Token not found in response"
**Solution**: Check that login endpoint returns `{ Token: "..." }` or `{ token: "..." }`. The helper handles both cases.

### Issue: "Connection refused"
**Solution**: Ensure `TestWebApplicationFactory` is properly configured and the test server is running.

### Issue: "User not found"
**Solution**: Ensure `TestDataSeeder.SeedTestDataAsync` is called in `TestWebApplicationFactory.ConfigureWebHost`.

## Example: Complete Test Class

```csharp
using FluentAssertions;
using System.Net;
using Xunit;
using ProjectManagementSystem.IntegrationTests.Helpers;

namespace ProjectManagementSystem.IntegrationTests.Controllers;

public class ExampleControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly string _authToken;

    public ExampleControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _authToken = AuthenticationHelper.GetAuthTokenAsync(_client).GetAwaiter().GetResult();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");
    }

    [Fact]
    public async Task Get_Authenticated_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/example");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_Unauthenticated_ReturnsUnauthorized()
    {
        var unauthenticatedClient = new HttpClient { BaseAddress = _client.BaseAddress };
        var response = await unauthenticatedClient.GetAsync("/api/example");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

