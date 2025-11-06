using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProjectManagementSystem.API.Attributes;

public class FileUploadValidationAttribute : ActionFilterAttribute
{
    private readonly long _maxFileSizeBytes;
    private readonly string[] _allowedExtensions;

    public FileUploadValidationAttribute(long maxFileSizeBytes = 10 * 1024 * 1024, string allowedExtensions = ".pdf,.doc,.docx,.xls,.xlsx,.txt,.jpg,.jpeg,.png,.gif")
    {
        _maxFileSizeBytes = maxFileSizeBytes;
        _allowedExtensions = allowedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(ext => ext.Trim().ToLowerInvariant())
            .ToArray();
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionArguments.TryGetValue("file", out var fileObj) && fileObj is Microsoft.AspNetCore.Http.IFormFile file)
        {
            // Check file size
            if (file.Length > _maxFileSizeBytes)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    Message = $"File size exceeds maximum allowed size of {_maxFileSizeBytes / (1024 * 1024)}MB"
                });
                return;
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                context.Result = new BadRequestObjectResult(new
                {
                    Message = $"File type not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}"
                });
                return;
            }

            // Check content type (basic validation)
            if (string.IsNullOrEmpty(file.ContentType))
            {
                context.Result = new BadRequestObjectResult(new
                {
                    Message = "File content type is required"
                });
                return;
            }
        }
    }
}

