using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Portfolio.Application.Auth.Interfaces;
using Portfolio.Application.Auth.Models;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Authentication;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenService(IOptions<JwtSettings> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _settings = options.Value;

        ValidateSettings(_settings);

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.Secret));

        _signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);
    }

    public AccessTokenResult CreateAccessToken(
        User user,
        IReadOnlyCollection<string> roles,
        DateTime issuedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(roles);

        var expiresAtUtc = issuedAtUtc.AddMinutes(
            _settings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new(
                "name",
                string.IsNullOrWhiteSpace(user.FullName)
                    ? user.Email
                    : user.FullName),

            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString("N"))
        };

        claims.AddRange(
            roles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(role => new Claim("role", role)));

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: issuedAtUtc,
            expires: expiresAtUtc,
            signingCredentials: _signingCredentials);

        var accessToken = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return new AccessTokenResult(
            accessToken,
            expiresAtUtc);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return WebEncoders.Base64UrlEncode(randomBytes);
    }

    public string HashRefreshToken(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        var tokenBytes = Encoding.UTF8.GetBytes(refreshToken);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }

    public DateTime GetRefreshTokenExpiry(DateTime issuedAtUtc)
    {
        return issuedAtUtc.AddDays(
            _settings.RefreshTokenExpiryDays);
    }

    private static void ValidateSettings(JwtSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Secret))
        {
            throw new InvalidOperationException(
                "JwtSettings:Secret không được để trống.");
        }

        if (Encoding.UTF8.GetByteCount(settings.Secret) < 32)
        {
            throw new InvalidOperationException(
                "JwtSettings:Secret phải có độ dài tối thiểu 32 byte.");
        }

        if (string.IsNullOrWhiteSpace(settings.Issuer))
        {
            throw new InvalidOperationException(
                "JwtSettings:Issuer không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(settings.Audience))
        {
            throw new InvalidOperationException(
                "JwtSettings:Audience không được để trống.");
        }

        if (settings.ExpiryMinutes <= 0)
        {
            throw new InvalidOperationException(
                "JwtSettings:ExpiryMinutes phải lớn hơn 0.");
        }

        if (settings.RefreshTokenExpiryDays <= 0)
        {
            throw new InvalidOperationException(
                "JwtSettings:RefreshTokenExpiryDays phải lớn hơn 0.");
        }
    }
}