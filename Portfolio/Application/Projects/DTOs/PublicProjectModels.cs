using Portfolio.Domain.Enums;

namespace Portfolio.Application.Projects.DTOs;

public sealed class PublicProjectFilterRequest
{
    public string? Keyword { get; set; }
    public string? Technology { get; set; }
    public ProjectStatus? Status { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public sealed class PublicProjectSkillDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;

    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;

    public SkillLevel? Level { get; init; }
    public string? LevelName { get; init; }
    public string? IconUrl { get; init; }
}

public sealed class PublicProjectImageDto
{
    public int Id { get; init; }
    public int ProjectId { get; init; }

    public string ImageUrl { get; init; } = string.Empty;
    public string? Caption { get; init; }

    public int DisplayOrder { get; init; }
    public bool IsThumbnail { get; init; }
}

public class PublicProjectCardDto
{
    public int Id { get; init; }

    public string ProjectName { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;

    public string? ShortDescription { get; init; }
    public string? ProjectType { get; init; }
    public string? ThumbnailUrl { get; init; }

    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }

    public ProjectStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;

    public bool IsFeatured { get; init; }

    public IReadOnlyList<PublicProjectSkillDto> Skills { get; init; } =
        Array.Empty<PublicProjectSkillDto>();
}

public sealed class PublicProjectDetailDto : PublicProjectCardDto
{
    public string? FullDescription { get; init; }
    public string? Role { get; init; }

    public string? GithubUrl { get; init; }
    public string? DemoUrl { get; init; }

    public IReadOnlyList<PublicProjectImageDto> Images { get; init; } =
        Array.Empty<PublicProjectImageDto>();
}

