using Microsoft.AspNetCore.Http;
using Portfolio.Application.Certificates.DTOs;
using Portfolio.Application.Common.Models;

namespace Portfolio.Application.Certificates.Interfaces;

public interface IAdminCertificateService
{
    Task<IReadOnlyList<CertificateDto>> GetAllAsync(CertificateFilterRequest filter, CancellationToken cancellationToken = default);
    Task<CertificateDto> CreateAsync(CertificateCreateRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<CertificateDto> UpdateAsync(int id, CertificateUpdateRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
    Task<CertificateDto> ToggleActiveAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
    Task<FileUrlResponse> UploadImageAsync(int id, IFormFile file, int currentUserId, CancellationToken cancellationToken = default);
}
