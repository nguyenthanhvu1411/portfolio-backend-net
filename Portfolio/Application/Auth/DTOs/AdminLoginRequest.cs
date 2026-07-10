namespace Portfolio.Application.Auth.DTOs;

public sealed class AdminLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
