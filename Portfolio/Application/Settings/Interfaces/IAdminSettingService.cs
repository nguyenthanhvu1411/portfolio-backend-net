using Microsoft.AspNetCore.Http;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Settings.DTOs;

namespace Portfolio.Application.Settings.Interfaces;

public interface IAdminSettingService
{
    Task<SettingDto> GetAsync(CancellationToken cancellationToken = default);
    Task<SettingDto> UpdateAsync(
        SettingUpdateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default);
    Task<FileUrlResponse> UploadLogoAsync(
        IFormFile file,
        int currentUserId,
        CancellationToken cancellationToken = default);
    Task<FileUrlResponse> UploadFaviconAsync(
        IFormFile file,
        int currentUserId,
        CancellationToken cancellationToken = default);
}
