using Portfolio.Domain.Enums;

namespace Portfolio.Application.Skills.DTOs;

public sealed class SkillDto
{
    public int Id { get; init; }
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public SkillLevel? Level { get; init; }
    public string? LevelName { get; init; }
    public string? IconUrl { get; init; }
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsFeatured { get; init; }
    public bool IsActive { get; init; }
    public int ProjectCount { get; init; }
}
