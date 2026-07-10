using Microsoft.AspNetCore.Http;

namespace Portfolio.Application.Projects.DTOs;

public sealed class ProjectImageUploadRequest
{
    public IFormFile File { get; set; } = null!;
    public string? Caption { get; set; }
    public int DisplayOrder { get; set; }
}
