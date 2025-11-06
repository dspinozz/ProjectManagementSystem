using Microsoft.Extensions.Logging;
using ProjectManagementSystem.Application.Interfaces;

namespace ProjectManagementSystem.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _logger = logger;
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            // Sanitize filename to prevent path traversal
            var sanitizedFileName = Path.GetFileName(fileName);
            if (string.IsNullOrEmpty(sanitizedFileName) || sanitizedFileName.Contains(".."))
            {
                throw new ArgumentException("Invalid file name", nameof(fileName));
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{sanitizedFileName}";
            var filePath = Path.GetFullPath(Path.Combine(_basePath, uniqueFileName));
            
            // Ensure the file path is within the base directory (prevent path traversal)
            if (!filePath.StartsWith(Path.GetFullPath(_basePath), StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException("Invalid file path");
            }

            using var fileStreamWriter = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(fileStreamWriter);

            _logger.LogInformation("File saved: {FilePath}", filePath);
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream> GetFileAsync(string filePath)
    {
        // Validate file path is within base directory (prevent path traversal)
        var fullPath = Path.GetFullPath(filePath);
        var baseFullPath = Path.GetFullPath(_basePath);
        
        if (!fullPath.StartsWith(baseFullPath, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Invalid file path");
        }

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            // Validate file path is within base directory (prevent path traversal)
            var fullPath = Path.GetFullPath(filePath);
            var baseFullPath = Path.GetFullPath(_basePath);
            
            if (!fullPath.StartsWith(baseFullPath, StringComparison.Ordinal))
            {
                _logger.LogWarning("Attempted to delete file outside base directory: {FilePath}", filePath);
                return Task.FromResult(false);
            }

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted: {FilePath}", fullPath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
            return Task.FromResult(false);
        }
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        return Task.FromResult(File.Exists(filePath));
    }
}

