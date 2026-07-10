namespace Portfolio.Application.Settings.DTOs;

public sealed class PublicSettingDto
{
    public string SiteName { get; init; } = string.Empty;

    public string? LogoUrl { get; init; }
    public string? FaviconUrl { get; init; }
    public string? ThemeColor { get; init; }

    public string? SeoTitle { get; init; }
    public string? SeoDescription { get; init; }

    public string? ContactEmail { get; init; }
    public string? FooterText { get; init; }

    // Social link hiện được lấy từ Profile đang active.
    public string? GithubUrl { get; init; }
    public string? LinkedinUrl { get; init; }
    public string? FacebookUrl { get; init; }
}

