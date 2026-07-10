using Microsoft.AspNetCore.Http;
using Portfolio.Application.Profiles.DTOs;

namespace Portfolio.Application.Profiles.Interfaces;

public interface IAdminProfileService
{
    Task<ProfileDto> GetAsync(CancellationToken cancellationToken = default);

    Task<ProfileDto> UpdateAsync(
        ProfileUpdateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<FileUploadResponse> UploadAvatarAsync(
        IFormFile file,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<FileUploadResponse> UploadBannerAsync(
        IFormFile file,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<FileUploadResponse> UploadCvAsync(
        IFormFile file,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task DeleteAvatarAsync(
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task DeleteBannerAsync(
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task DeleteCvAsync(
        int currentUserId,
        CancellationToken cancellationToken = default);
}
