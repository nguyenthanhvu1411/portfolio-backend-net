using Portfolio.Application.Common.Models;
using Portfolio.Application.Projects.DTOs;

namespace Portfolio.Application.Projects.Interfaces;

public interface IPublicProjectService
{
    Task<IReadOnlyList<PublicProjectCardDto>> GetFeaturedAsync(
        int limit,
        CancellationToken cancellationToken = default);

    Task<PagedResult<PublicProjectCardDto>> GetPagedAsync(
        PublicProjectFilterRequest filter,
        CancellationToken cancellationToken = default);

    Task<PublicProjectDetailDto> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PublicProjectImageDto>> GetImagesAsync(
        int projectId,
        CancellationToken cancellationToken = default);
}

