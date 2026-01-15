using System.Net;
using System.Net.Http.Json;
using ProjectManagementSystem.UI.Models;

namespace ProjectManagementSystem.UI.Services;

public interface IFileService
{
    Task<List<ProjectFileDto>> GetProjectFilesAsync(string projectId);
    Task<bool> UploadFileAsync(string projectId, Stream fileStream, string fileName, string contentType);
    Task<Stream?> DownloadFileAsync(string fileId);
    Task<bool> DeleteFileAsync(string fileId);
}

public class FileService : IFileService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthService _authService;

    public FileService(IHttpClientFactory httpClientFactory, IAuthService authService)
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

    public async Task<List<ProjectFileDto>> GetProjectFilesAsync(string projectId)
    {
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Get, $"/api/files/project/{projectId}");
        var response = await _httpClient.SendAsync(httpRequest);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<List<ProjectFileDto>>() ?? new();
        
        throw new HttpRequestException($"Failed to load files: {response.StatusCode}");
    }

    public async Task<bool> UploadFileAsync(string projectId, Stream fileStream, string fileName, string contentType)
    {
        var token = await _authService.GetTokenAsync();
        
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);
        
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/files/upload/{projectId}");
        request.Content = content;
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        
        var response = await _httpClient.SendAsync(request);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        
        return response.IsSuccessStatusCode;
    }

    public async Task<Stream?> DownloadFileAsync(string fileId)
    {
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Get, $"/api/files/{fileId}/download");
        var response = await _httpClient.SendAsync(httpRequest);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;
        
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsStreamAsync();
        
        throw new HttpRequestException($"Failed to download file: {response.StatusCode}");
    }

    public async Task<bool> DeleteFileAsync(string fileId)
    {
        var httpRequest = await CreateAuthenticatedRequestAsync(HttpMethod.Delete, $"/api/files/{fileId}");
        var response = await _httpClient.SendAsync(httpRequest);
        
        if (response.StatusCode == HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException("Session expired. Please login again.");
        
        return response.IsSuccessStatusCode;
    }
}
