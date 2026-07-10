using Portfolio.Domain.Enums;

namespace Portfolio.Application.Users.DTOs;

public sealed class UserFilterRequest
{
    public string? Keyword { get; set; }
    public UserStatus? Status { get; set; }
}

public sealed class UserRoleDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public sealed class UserDto
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public UserStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public IReadOnlyList<UserRoleDto> Roles { get; init; } = Array.Empty<UserRoleDto>();
}

public sealed class UserCreateRequest
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public IReadOnlyCollection<int> RoleIds { get; set; } = Array.Empty<int>();
}

public sealed class UserUpdateRequest
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public IReadOnlyCollection<int> RoleIds { get; set; } = Array.Empty<int>();
}

public sealed class ResetUserPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
