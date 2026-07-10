using Portfolio.Application.Common.Models;
using Portfolio.Application.Users.DTOs;

namespace Portfolio.Application.Users.Interfaces;

public interface IAdminUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(
        UserFilterRequest filter,
        CancellationToken cancellationToken = default);

    Task<UserDto> CreateAsync(
        UserCreateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<UserDto> UpdateAsync(
        int id,
        UserUpdateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<UserDto> LockAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<UserDto> UnlockAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<OperationResult> ResetPasswordAsync(
        int id,
        ResetUserPasswordRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default);
}
