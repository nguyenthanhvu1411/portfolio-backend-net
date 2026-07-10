using Microsoft.AspNetCore.Http;
using Portfolio.Application.Blogs.DTOs;
using Portfolio.Application.Common.Models;

namespace Portfolio.Application.Blogs.Interfaces;

public interface IAdminBlogCategoryService
{
    Task<IReadOnlyList<BlogCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BlogCategoryDto> CreateAsync(BlogCategoryCreateRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<BlogCategoryDto> UpdateAsync(int id, BlogCategoryUpdateRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
}

public interface IAdminBlogService
{
    Task<PagedResult<BlogDto>> GetPagedAsync(BlogFilterRequest filter, CancellationToken cancellationToken = default);
    Task<BlogDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<BlogDto> CreateAsync(BlogCreateRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<BlogDto> UpdateAsync(int id, BlogUpdateRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
    Task<BlogDto> PublishAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
    Task<BlogDto> UnpublishAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
    Task<BlogDto> ToggleFeaturedAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
    Task<FileUrlResponse> UploadThumbnailAsync(int id, IFormFile file, int currentUserId, CancellationToken cancellationToken = default);
}
