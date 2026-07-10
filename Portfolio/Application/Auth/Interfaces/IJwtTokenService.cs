using Portfolio.Application.Auth.Models;
using Portfolio.Domain.Entities;

namespace Portfolio.Application.Auth.Interfaces;

public interface IJwtTokenService
{
    AccessTokenResult CreateAccessToken(
        User user,
        IReadOnlyCollection<string> roles,
        DateTime issuedAtUtc);

    string GenerateRefreshToken();

    string HashRefreshToken(string refreshToken);

    DateTime GetRefreshTokenExpiry(DateTime issuedAtUtc);
}
