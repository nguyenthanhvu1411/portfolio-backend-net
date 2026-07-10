using Portfolio.Domain.Enums;

namespace Portfolio.Application.Projects.DTOs;

public sealed class ProjectSkillDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public SkillLevel? Level { get; set; }
    public string? LevelName { get; set; }
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; }
}
