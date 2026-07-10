namespace Portfolio.Application.Skills.DTOs;

public sealed class SkillCategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public int SkillCount { get; init; }
}
