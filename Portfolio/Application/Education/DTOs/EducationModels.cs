namespace Portfolio.Application.Education.DTOs;

public sealed class EducationDto
{
    public int Id { get; init; }
    public string SchoolName { get; init; } = string.Empty;
    public string Major { get; init; } = string.Empty;
    public string? Degree { get; init; }
    public int? StartYear { get; init; }
    public int? EndYear { get; init; }
    public string? GPA { get; init; }
    public string? Description { get; init; }
    public string? LogoUrl { get; init; }
    public bool IsActive { get; init; }
}

public class EducationCreateRequest
{
    public string SchoolName { get; set; } = string.Empty;
    public string Major { get; set; } = string.Empty;
    public string? Degree { get; set; }
    public int? StartYear { get; set; }
    public int? EndYear { get; set; }
    public string? GPA { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class EducationUpdateRequest : EducationCreateRequest
{
}
