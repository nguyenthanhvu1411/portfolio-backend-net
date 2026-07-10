using Microsoft.AspNetCore.Http;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Projects.DTOs;

namespace Portfolio.Application.Projects.Interfaces;

public interface IAdminProjectService
{
    Task<PagedResult<ProjectDto>> GetPagedAsync(
        ProjectFilterRequest filter,
        CancellationToken cancellationToken = default);

    Task<ProjectDetailDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<ProjectDto> CreateAsync(
        ProjectCreateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<ProjectDto> UpdateAsync(
        int id,
        ProjectUpdateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<OperationResult> DeleteAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<ProjectDto> ToggleActiveAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<ProjectDto> ToggleFeaturedAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<ProjectFileUploadResponse> UploadThumbnailAsync(
        int projectId,
        IFormFile file,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ProjectImageDto>> GetImagesAsync(
        int projectId,
        CancellationToken cancellationToken = default);

    Task<ProjectImageDto> UploadImageAsync(
        int projectId,
        ProjectImageUploadRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<ProjectImageDto> UpdateImageAsync(
        int imageId,
        ProjectImageUpdateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<OperationResult> DeleteImageAsync(
        int imageId,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<OperationResult> SetThumbnailAsync(
        int imageId,
        int currentUserId,
        CancellationToken cancellationToken = default);
}
