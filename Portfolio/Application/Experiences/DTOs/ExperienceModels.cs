namespace Portfolio.Application.Experiences.DTOs;

public sealed class ExperienceFilterRequest
{
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class ExperienceDto
{
    public int Id { get; init; }
    public string Position { get; init; } = string.Empty;
    public string Company { get; init; } = string.Empty;
    public string? CompanyLogoUrl { get; init; }
    public string? Location { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool IsCurrent { get; init; }
    public string? Description { get; init; }
    public string? Technologies { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
}

public class ExperienceCreateRequest
{
    public string Position { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string? CompanyLogoUrl { get; set; }
    public string? Location { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsCurrent { get; set; }
    public string? Description { get; set; }
    public string? Technologies { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ExperienceUpdateRequest : ExperienceCreateRequest
{
}
