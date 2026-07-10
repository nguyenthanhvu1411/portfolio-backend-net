namespace Portfolio.Application.Auth.DTOs;

public sealed class AdminLoginResponse
{
    public string TokenType { get; init; } = "Bearer";
    public string AccessToken { get; init; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; init; }
    public CurrentAdminResponse Admin { get; init; } = new();
}
