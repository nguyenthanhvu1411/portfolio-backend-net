using Portfolio.Domain.Enums;

namespace Portfolio.Application.Projects.DTOs;

public sealed class ProjectCreateRequest
{
    public string ProjectName { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public string? Role { get; set; }
    public string? ProjectType { get; set; }
    public string? GithubUrl { get; set; }
    public string? DemoUrl { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public IReadOnlyCollection<int> SkillIds { get; set; } = [];
}
