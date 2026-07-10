namespace Portfolio.Application.Profiles.DTOs;

public sealed class ProfileUpdateRequest
{
    public string FullName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string? ShortBio { get; set; }
    public string? AboutMe { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? GithubUrl { get; set; }
    public string? LinkedinUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
