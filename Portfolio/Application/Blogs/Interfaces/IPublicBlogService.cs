using Portfolio.Application.Common.Models;
using Portfolio.Application.Blogs.DTOs;

namespace Portfolio.Application.Blogs.Interfaces;

public interface IPublicBlogService
{
    Task<PagedResult<PublicBlogCardDto>> GetPagedAsync(
        PublicBlogFilterRequest filter,
        CancellationToken cancellationToken = default);

    Task<PublicBlogDetailDto> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<OperationResult> IncreaseViewAsync(
        int blogId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PublicBlogCategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken = default);
}

