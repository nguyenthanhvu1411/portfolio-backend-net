using Portfolio.Application.Auth.DTOs;
using Portfolio.Application.Auth.Models;

namespace Portfolio.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthSession?> LoginAsync(
        AdminLoginRequest request,
        CancellationToken cancellationToken = default);

    Task<AuthSession?> RefreshAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task LogoutAsync(
        string? refreshToken,
        CancellationToken cancellationToken = default);

    Task<CurrentAdminResponse?> GetCurrentAdminAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
