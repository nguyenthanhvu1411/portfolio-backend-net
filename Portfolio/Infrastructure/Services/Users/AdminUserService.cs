using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Users.DTOs;
using Portfolio.Application.Users.Interfaces;
using Portfolio.Common.Exceptions;
using Portfolio.Domain.Common;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Services.Users;

public sealed class AdminUserService : IAdminUserService
{
    private static readonly string[] AllowedAdminRoles = { "SuperAdmin", "Admin" };

    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AdminUserService(
        ApplicationDbContext dbContext,
        IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(
        UserFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Users
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();
            query = query.Where(x =>
                x.Email.Contains(keyword) ||
                x.FullName.Contains(keyword));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        var users = await query
            .OrderByDescending(x => x.UserRoles.Any(ur => ur.Role.Name == "SuperAdmin"))
            .ThenBy(x => x.FullName)
            .ThenBy(x => x.Email)
            .ToListAsync(cancellationToken);

        return users.Select(Map).ToArray();
    }

    public async Task<UserDto> CreateAsync(
        UserCreateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);

        if (await _dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken))
        {
            throw new ConflictException("Email đã được sử dụng bởi tài khoản khác.");
        }

        var roles = await GetAllowedRolesAsync(request.RoleIds, cancellationToken);

        var user = new User
        {
            Email = email,
            FullName = request.FullName.Trim(),
            Status = request.Status,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole { RoleId = role.Id });
        }

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = await GetByIdAsync(user.Id, cancellationToken);
        AddAuditLog(currentUserId, AuditAction.Create, user.Id, null, result);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<UserDto> UpdateAsync(
        int id,
        UserUpdateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var user = await GetTrackedWithRolesAsync(id, cancellationToken);
        var email = NormalizeEmail(request.Email);

        if (await _dbContext.Users.AnyAsync(
                x => x.Email == email && x.Id != id,
                cancellationToken))
        {
            throw new ConflictException("Email đã được sử dụng bởi tài khoản khác.");
        }

        var roles = await GetAllowedRolesAsync(request.RoleIds, cancellationToken);

        await EnsureLastSuperAdminIsPreservedAsync(
            user,
            request.Status,
            roles.Select(x => x.Name).ToArray(),
            cancellationToken);

        if (id == currentUserId && request.Status != UserStatus.Active)
        {
            throw new ConflictException(
                "Bạn không thể khóa hoặc ngưng hoạt động tài khoản đang đăng nhập.");
        }

        var oldValue = Map(user);

        user.Email = email;
        user.FullName = request.FullName.Trim();
        user.AvatarUrl = TrimToNull(request.AvatarUrl);
        user.Status = request.Status;
        user.UpdatedAt = DateTime.UtcNow;

        var requestedRoleIds = roles.Select(x => x.Id).ToHashSet();

        var removedRoles = user.UserRoles
            .Where(x => !requestedRoleIds.Contains(x.RoleId))
            .ToArray();

        _dbContext.UserRoles.RemoveRange(removedRoles);

        foreach (var removedRole in removedRoles)
        {
            user.UserRoles.Remove(removedRole);
        }

        var existingRoleIds = user.UserRoles
            .Select(x => x.RoleId)
            .ToHashSet();

        foreach (var role in roles.Where(x => !existingRoleIds.Contains(x.Id)))
        {
            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                Role = role
            });
        }

        if (request.Status != UserStatus.Active)
        {
            await RevokeRefreshTokensAsync(user.Id, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = await GetByIdAsync(user.Id, cancellationToken);
        AddAuditLog(currentUserId, AuditAction.Update, user.Id, oldValue, result);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<UserDto> LockAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        if (id == currentUserId)
        {
            throw new ConflictException("Bạn không thể khóa tài khoản đang đăng nhập.");
        }

        var user = await GetTrackedWithRolesAsync(id, cancellationToken);

        await EnsureLastSuperAdminIsPreservedAsync(
            user,
            UserStatus.Locked,
            user.UserRoles.Select(x => x.Role.Name).ToArray(),
            cancellationToken);

        if (user.Status == UserStatus.Locked)
        {
            return Map(user);
        }

        var oldValue = Map(user);
        user.Status = UserStatus.Locked;
        user.UpdatedAt = DateTime.UtcNow;

        await RevokeRefreshTokensAsync(user.Id, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = Map(user);
        AddAuditLog(currentUserId, AuditAction.Update, user.Id, oldValue, result);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<UserDto> UnlockAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var user = await GetTrackedWithRolesAsync(id, cancellationToken);

        if (user.Status == UserStatus.Active)
        {
            return Map(user);
        }

        var oldValue = Map(user);
        user.Status = UserStatus.Active;
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = Map(user);
        AddAuditLog(currentUserId, AuditAction.Update, user.Id, oldValue, result);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<OperationResult> ResetPasswordAsync(
        int id,
        ResetUserPasswordRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(
            x => x.Id == id,
            cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy tài khoản có Id = {id}.");

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await RevokeRefreshTokensAsync(user.Id, cancellationToken);

        AddAuditLog(
            currentUserId,
            AuditAction.Update,
            user.Id,
            new { Password = "***" },
            new { PasswordResetAt = user.UpdatedAt, RefreshTokensRevoked = true });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new OperationResult
        {
            Success = true,
            Message = "Đã đặt lại mật khẩu và thu hồi các phiên đăng nhập cũ."
        };
    }

    private async Task<UserDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .SingleAsync(x => x.Id == id, cancellationToken);

        return Map(user);
    }

    private async Task<User> GetTrackedWithRolesAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy tài khoản có Id = {id}.");
    }

    private async Task<IReadOnlyList<Role>> GetAllowedRolesAsync(
        IReadOnlyCollection<int> roleIds,
        CancellationToken cancellationToken)
    {
        var ids = roleIds.Distinct().ToArray();
        var roles = await _dbContext.Roles
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (roles.Count != ids.Length)
        {
            throw new RequestValidationException("roleIds", "Có vai trò không tồn tại.");
        }

        if (roles.Any(x => !AllowedAdminRoles.Contains(
                x.Name,
                StringComparer.OrdinalIgnoreCase)))
        {
            throw new RequestValidationException(
                "roleIds", "Chỉ được gán vai trò Admin hoặc SuperAdmin.");
        }

        return roles;
    }

    private async Task EnsureLastSuperAdminIsPreservedAsync(
        User user,
        UserStatus targetStatus,
        IReadOnlyCollection<string> targetRoleNames,
        CancellationToken cancellationToken)
    {
        var currentlyActiveSuperAdmin =
            user.Status == UserStatus.Active &&
            user.UserRoles.Any(x => x.Role.Name == "SuperAdmin");

        var remainsActiveSuperAdmin =
            targetStatus == UserStatus.Active &&
            targetRoleNames.Contains("SuperAdmin", StringComparer.OrdinalIgnoreCase);

        if (!currentlyActiveSuperAdmin || remainsActiveSuperAdmin)
        {
            return;
        }

        var otherExists = await _dbContext.Users.AnyAsync(
            x => x.Id != user.Id &&
                 x.Status == UserStatus.Active &&
                 x.UserRoles.Any(ur => ur.Role.Name == "SuperAdmin"),
            cancellationToken);

        if (!otherExists)
        {
            throw new ConflictException(
                "Không thể thay đổi SuperAdmin hoạt động cuối cùng của hệ thống.");
        }
    }

    private Task<int> RevokeRefreshTokensAsync(int userId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        return _dbContext.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.RevokedAt, now),
                cancellationToken);
    }

    private void AddAuditLog(
        int currentUserId,
        AuditAction action,
        int entityId,
        object? oldValue,
        object? newValue)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = currentUserId,
            Action = action,
            EntityName = nameof(User),
            EntityId = entityId.ToString(),
            OldValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static UserDto Map(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            Status = user.Status,
            StatusName = user.Status.GetDisplayName(),
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = user.UserRoles
                .Select(x => x.Role)
                .OrderBy(x => x.Name)
                .Select(x => new UserRoleDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description
                })
                .ToArray()
        };
    }

    private static string NormalizeEmail(string value) => value.Trim().ToLowerInvariant();
    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
