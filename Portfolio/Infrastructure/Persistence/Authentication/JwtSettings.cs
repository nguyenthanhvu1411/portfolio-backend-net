namespace Portfolio.Infrastructure.Authentication;

public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string Secret { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int RefreshTokenExpiryDays { get; set; } = 7;
}