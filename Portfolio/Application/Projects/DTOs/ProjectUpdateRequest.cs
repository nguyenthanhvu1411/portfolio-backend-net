using Portfolio.Domain.Enums;

namespace Portfolio.Application.Projects.DTOs;

public sealed class ProjectUpdateRequest
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
    public ProjectStatus Status { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyCollection<int> SkillIds { get; set; } = [];
}
