using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using ProjectManagementSystem.IntegrationTests.Helpers;

namespace ProjectManagementSystem.IntegrationTests.Controllers;

public class FilesControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly string _authToken;
    private readonly Guid _testProjectId;

    public FilesControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        _authToken = AuthenticationHelper.GetAuthTokenAsync(_client).GetAwaiter().GetResult();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_authToken}");
        _testProjectId = TestDataSeeder.TestProjectId;
    }

    [Fact]
    public async System.Threading.Tasks.Task UploadFile_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = new HttpClient { BaseAddress = _client.BaseAddress };
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "file", "test.txt");

        // Act
        var response = await client.PostAsync($"/api/files/upload/{_testProjectId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async System.Threading.Tasks.Task UploadFile_NoFile_ReturnsBadRequest()
    {
        // Arrange
        var content = new MultipartFormDataContent();

        // Act
        var response = await _client.PostAsync($"/api/files/upload/{_testProjectId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetProjectFiles_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = new HttpClient { BaseAddress = _client.BaseAddress };

        // Act
        var response = await client.GetAsync($"/api/files/project/{_testProjectId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetProjectFiles_Authenticated_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync($"/api/files/project/{_testProjectId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteFile_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = new HttpClient { BaseAddress = _client.BaseAddress };

        // Act
        var response = await client.DeleteAsync($"/api/files/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
