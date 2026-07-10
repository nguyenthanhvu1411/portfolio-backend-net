namespace Portfolio.Application.Profiles.DTOs;

public sealed class PublicProfileDto
{
    public int Id { get; init; }

    public string FullName { get; init; } = string.Empty;
    public string JobTitle { get; init; } = string.Empty;

    public string? ShortBio { get; init; }
    public string? AboutMe { get; init; }

    public string? AvatarUrl { get; init; }
    public string? BannerUrl { get; init; }

    public string? CvUrl { get; init; }
    public string? CvFileName { get; init; }
    public string? CvContentType { get; init; }
    public long? CvFileSize { get; init; }

    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }

    public string? GithubUrl { get; init; }
    public string? LinkedinUrl { get; init; }
    public string? FacebookUrl { get; init; }
}

