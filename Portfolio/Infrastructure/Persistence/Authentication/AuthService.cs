using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Auth.DTOs;
using Portfolio.Application.Auth.Interfaces;
using Portfolio.Application.Auth.Models;
using Portfolio.Application.Common.Security;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Authentication;

public sealed class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        ApplicationDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthSession?> LoginAsync(
        AdminLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(
                x => x.Email == normalizedEmail,
                cancellationToken);

        // Trả cùng một kết quả cho email sai, mật khẩu sai, tài khoản bị khóa
        // hoặc tài khoản không có quyền Admin để tránh lộ thông tin tài khoản.
        if (user is null ||
            user.Status != UserStatus.Active ||
            !HasAdminRole(user))
        {
            return null;
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var now = DateTime.UtcNow;

        if (passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        }

        user.LastLoginAt = now;
        user.UpdatedAt = now;

        // Dọn token cũ để bảng RefreshTokens không tăng vô hạn.
        await _dbContext.RefreshTokens
            .Where(x =>
                x.UserId == user.Id &&
                (x.ExpiresAt <= now || x.RevokedAt != null))
            .ExecuteDeleteAsync(cancellationToken);

        var session = CreateSession(user, now);

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = _jwtTokenService.HashRefreshToken(session.RefreshToken),
            ExpiresAt = session.RefreshTokenExpiresAt
        });

        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = AuditAction.Login,
            EntityName = nameof(User),
            EntityId = user.Id.ToString(),
            NewValue = "Admin login successful",
            CreatedAt = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task<AuthSession?> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var tokenHash = _jwtTokenService.HashRefreshToken(refreshToken);
        var now = DateTime.UtcNow;

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        var storedToken = await _dbContext.RefreshTokens
            .AsNoTracking()
            .Include(x => x.User)
                .ThenInclude(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(
                x => x.Token == tokenHash,
                cancellationToken);

        if (storedToken is null ||
            storedToken.RevokedAt != null ||
            storedToken.ExpiresAt <= now ||
            storedToken.User.Status != UserStatus.Active ||
            !HasAdminRole(storedToken.User))
        {
            return null;
        }

        // Refresh-token rotation: chỉ một request được phép sử dụng token cũ.
        var affectedRows = await _dbContext.RefreshTokens
            .Where(x =>
                x.Id == storedToken.Id &&
                x.RevokedAt == null &&
                x.ExpiresAt > now)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.RevokedAt, now),
                cancellationToken);

        if (affectedRows != 1)
        {
            return null;
        }

        var session = CreateSession(storedToken.User, now);

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = storedToken.UserId,
            Token = _jwtTokenService.HashRefreshToken(session.RefreshToken),
            ExpiresAt = session.RefreshTokenExpiresAt
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return session;
    }

    public async Task LogoutAsync(
        string? refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var tokenHash = _jwtTokenService.HashRefreshToken(refreshToken);
        var now = DateTime.UtcNow;

        await _dbContext.RefreshTokens
            .Where(x =>
                x.Token == tokenHash &&
                x.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.RevokedAt, now),
                cancellationToken);
    }

    public async Task<CurrentAdminResponse?> GetCurrentAdminAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(
                x => x.Id == userId && x.Status == UserStatus.Active,
                cancellationToken);

        if (user is null || !HasAdminRole(user))
        {
            return null;
        }

        return MapAdmin(user);
    }

    private AuthSession CreateSession(User user, DateTime issuedAtUtc)
    {
        var roles = user.UserRoles
            .Select(x => x.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToArray();

        var accessToken = _jwtTokenService.CreateAccessToken(
            user,
            roles,
            issuedAtUtc);

        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenExpiresAt = _jwtTokenService.GetRefreshTokenExpiry(issuedAtUtc);

        return new AuthSession(
            accessToken,
            refreshToken,
            refreshTokenExpiresAt,
            MapAdmin(user));
    }

    private static CurrentAdminResponse MapAdmin(User user)
    {
        return new CurrentAdminResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            Roles = user.UserRoles
                .Select(x => x.Role.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToArray()
        };
    }

    private static bool HasAdminRole(User user)
    {
        return user.UserRoles.Any(userRole =>
            AuthPolicies.AdminRoles.Contains(
                userRole.Role.Name,
                StringComparer.OrdinalIgnoreCase));
    }
}
