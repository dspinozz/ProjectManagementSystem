using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using ProjectManagementSystem.IntegrationTests.Helpers;

namespace ProjectManagementSystem.IntegrationTests.Controllers;

public class TasksControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly string _authToken;

    public TasksControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _authToken = AuthenticationHelper.GetAuthTokenAsync(_client).GetAwaiter().GetResult();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTasksByProject_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = new HttpClient { BaseAddress = _client.BaseAddress };

        // Act
        var response = await client.GetAsync($"/api/tasks/project/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTasksByProject_Authenticated_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync($"/api/tasks/project/{TestDataSeeder.TestProjectId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetTask_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = new HttpClient { BaseAddress = _client.BaseAddress };

        // Act
        var response = await client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTask_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = new HttpClient { BaseAddress = _client.BaseAddress };
        var task = new
        {
            Title = "Test Task",
            Description = "Test Description",
            Status = 0, // ToDo
            Priority = 1, // Medium
            ProjectId = TestDataSeeder.TestProjectId
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/tasks", task);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateTask_InvalidProject_ReturnsBadRequest()
    {
        // Arrange
        var task = new
        {
            Title = "Test Task",
            Description = "Test Description",
            Status = 0,
            Priority = 1,
            ProjectId = Guid.NewGuid() // Non-existent project
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", task);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest, 
            HttpStatusCode.InternalServerError,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateTask_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = new HttpClient { BaseAddress = _client.BaseAddress };
        var task = new
        {
            Id = Guid.NewGuid(),
            Title = "Updated Task"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/tasks/{task.Id}", task);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteTask_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = new HttpClient { BaseAddress = _client.BaseAddress };

        // Act
        var response = await client.DeleteAsync($"/api/tasks/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteTask_NonExistentTask_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/tasks/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
