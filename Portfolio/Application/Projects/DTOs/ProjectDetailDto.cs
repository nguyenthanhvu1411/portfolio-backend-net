namespace Portfolio.Application.Projects.DTOs;

public sealed class ProjectDetailDto : ProjectDto
{
    public IReadOnlyCollection<ProjectSkillDto> Skills { get; set; } = [];
    public IReadOnlyCollection<ProjectImageDto> Images { get; set; } = [];
}
