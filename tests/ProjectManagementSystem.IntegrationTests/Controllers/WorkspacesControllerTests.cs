using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using ProjectManagementSystem.IntegrationTests.Helpers;

namespace ProjectManagementSystem.IntegrationTests.Controllers;

public class WorkspacesControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly string _authToken;

    public WorkspacesControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _authToken = AuthenticationHelper.GetAuthTokenAsync(_client).GetAwaiter().GetResult();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");
    }

    [Fact]
    public async System.Threading.Tasks.Task GetWorkspaces_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = new HttpClient { BaseAddress = _client.BaseAddress };

        // Act
        var response = await client.GetAsync("/api/workspaces");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetWorkspaces_Authenticated_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/workspaces");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetWorkspace_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = new HttpClient { BaseAddress = _client.BaseAddress };

        // Act
        var response = await client.GetAsync($"/api/workspaces/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetWorkspace_Existing_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync($"/api/workspaces/{TestDataSeeder.TestWorkspaceId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetWorkspace_NonExistent_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/workspaces/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateWorkspace_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = new HttpClient { BaseAddress = _client.BaseAddress };
        var workspace = new
        {
            Name = "New Workspace",
            Description = "Test Description",
            OrganizationId = TestDataSeeder.TestOrganizationId
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/workspaces", workspace);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
