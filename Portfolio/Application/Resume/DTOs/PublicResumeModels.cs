namespace Portfolio.Application.Resume.DTOs;

public sealed class PublicExperienceDto
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
}

public sealed class PublicEducationDto
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
}

public sealed class PublicCertificateFilterRequest
{
    public string? Keyword { get; set; }
    public string? Organization { get; set; }
}

public sealed class PublicCertificateDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
    public string Organization { get; init; } = string.Empty;

    public DateTime? IssueDate { get; init; }
    public DateTime? ExpiryDate { get; init; }

    public string? CredentialId { get; init; }
    public string? CredentialUrl { get; init; }
    public string? ImageUrl { get; init; }
    public string? Description { get; init; }
}
