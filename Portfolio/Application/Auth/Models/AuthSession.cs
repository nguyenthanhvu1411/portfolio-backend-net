using Portfolio.Application.Auth.DTOs;

namespace Portfolio.Application.Auth.Models;

public sealed record AuthSession(
    AccessTokenResult AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    CurrentAdminResponse Admin);
