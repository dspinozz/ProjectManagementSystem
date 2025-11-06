using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using ProjectManagementSystem.IntegrationTests.Helpers;

namespace ProjectManagementSystem.IntegrationTests.Controllers;

public class ProjectsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly string _authToken;

    public ProjectsControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _authToken = AuthenticationHelper.GetAuthTokenAsync(_client).GetAwaiter().GetResult();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");
    }

    [Fact]
    public async System.Threading.Tasks.Task GetProjects_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = new HttpClient { BaseAddress = _client.BaseAddress };

        // Act
        var response = await client.GetAsync("/api/projects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetProjects_Authenticated_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/projects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetProject_Existing_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync($"/api/projects/{TestDataSeeder.TestProjectId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetProject_NonExistent_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/projects/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
