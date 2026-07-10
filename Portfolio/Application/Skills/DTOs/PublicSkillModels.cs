using Portfolio.Domain.Enums;

namespace Portfolio.Application.Skills.DTOs;

public sealed class PublicSkillFilterRequest
{
    public int? CategoryId { get; set; }
    public string? Keyword { get; set; }
}


public sealed class PublicSkillDto
{
    public int Id { get; init; }

    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public int CategoryDisplayOrder { get; init; }

    public string Name { get; init; } = string.Empty;

    public SkillLevel? Level { get; init; }
    public string? LevelName { get; init; }

    public string? IconUrl { get; init; }
    public string? Description { get; init; }

    public int DisplayOrder { get; init; }
    public bool IsFeatured { get; init; }
}

public sealed class PublicSkillCategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
    public int SkillCount { get; init; }
}


