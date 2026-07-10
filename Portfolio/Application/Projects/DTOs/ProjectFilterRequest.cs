using Portfolio.Domain.Enums;

namespace Portfolio.Application.Projects.DTOs;

public sealed class ProjectFilterRequest
{
    public string? Keyword { get; set; }
    public ProjectStatus? Status { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
