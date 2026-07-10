using Microsoft.AspNetCore.Http;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Uploads.DTOs;

namespace Portfolio.Application.Uploads.Interfaces;

public interface IAdminUploadService
{
    Task<UploadedFileDto> UploadImageAsync(IFormFile file, int currentUserId, CancellationToken cancellationToken = default);
    Task<UploadedFileDto> UploadFileAsync(IFormFile file, int currentUserId, CancellationToken cancellationToken = default);
    Task<UploadedFileDto> UploadCvAsync(IFormFile file, int currentUserId, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(int fileId, int currentUserId, CancellationToken cancellationToken = default);
}
