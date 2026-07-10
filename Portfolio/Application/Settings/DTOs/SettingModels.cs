namespace Portfolio.Application.Settings.DTOs;

public sealed class SettingDto
{
    public int Id { get; init; }
    public string SiteName { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string? FaviconUrl { get; init; }
    public string? ThemeColor { get; init; }
    public string? SeoTitle { get; init; }
    public string? SeoDescription { get; init; }
    public string? ContactEmail { get; init; }
    public string? FooterText { get; init; }
}

public sealed class SettingUpdateRequest
{
    public string SiteName { get; set; } = string.Empty;
    public string? ThemeColor { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? ContactEmail { get; set; }
    public string? FooterText { get; set; }
}
