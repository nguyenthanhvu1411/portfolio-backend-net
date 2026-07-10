using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Blogs.DTOs;
using Portfolio.Application.Blogs.Interfaces;
using Portfolio.Common.Exceptions;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Services.Blogs;

public sealed class PublicBlogService
    : IPublicBlogService
{
    private readonly ApplicationDbContext _dbContext;

    public PublicBlogService(
        ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<PublicBlogCardDto>>
        GetPagedAsync(
            PublicBlogFilterRequest filter,
            CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var query = _dbContext.Blogs
            .AsNoTracking()
            .Where(x =>
                x.Status == BlogStatus.Published &&
                x.Category.IsActive &&
                x.PublishedAt.HasValue &&
                x.PublishedAt <= now)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();

            query = query.Where(x =>
                x.Title.Contains(keyword) ||
                x.Slug.Contains(keyword) ||
                (x.Summary != null &&
                 x.Summary.Contains(keyword)) ||
                x.Content.Contains(keyword));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(x =>
                x.CategoryId ==
                filter.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Tag))
        {
            var tag = filter.Tag.Trim();

            query = query.Where(x =>
                x.BlogTagMappings.Any(mapping =>
                    mapping.Tag.Name == tag ||
                    mapping.Tag.Slug == tag));
        }

        var totalCount = await query.CountAsync(
            cancellationToken);

        var blogs = await query
            .Include(x => x.Category)
            .Include(x => x.BlogTagMappings)
                .ThenInclude(x => x.Tag)
            .OrderByDescending(x => x.IsFeatured)
            .ThenByDescending(x => x.PublishedAt)
            .ThenByDescending(x => x.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<PublicBlogCardDto>.Create(
            blogs.Select(MapCard).ToArray(),
            filter.Page,
            filter.PageSize,
            totalCount);
    }

    public async Task<PublicBlogDetailDto> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim();
        var now = DateTime.UtcNow;

        var blog = await _dbContext.Blogs
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.BlogTagMappings)
                .ThenInclude(x => x.Tag)
            .SingleOrDefaultAsync(
                x =>
                    x.Slug == normalizedSlug &&
                    x.Status == BlogStatus.Published &&
                    x.Category.IsActive &&
                    x.PublishedAt.HasValue &&
                    x.PublishedAt <= now,
                cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy bài viết public có slug '{normalizedSlug}'.");

        return MapDetail(blog);
    }

    public async Task<OperationResult> IncreaseViewAsync(
        int blogId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var affectedRows = await _dbContext.Blogs
            .Where(x =>
                x.Id == blogId &&
                x.Status == BlogStatus.Published &&
                x.Category.IsActive &&
                x.PublishedAt.HasValue &&
                x.PublishedAt <= now)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    x => x.ViewCount,
                    x => x.ViewCount + 1),
                cancellationToken);

        if (affectedRows == 0)
        {
            throw new NotFoundException(
                $"Không tìm thấy bài viết public có Id = {blogId}.");
        }

        return new OperationResult
        {
            Success = true,
            Message = "Đã tăng lượt xem bài viết."
        };
    }

    public async Task<IReadOnlyList<PublicBlogCategoryDto>>
        GetCategoriesAsync(
            CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _dbContext.BlogCategories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new PublicBlogCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                PublishedBlogCount = x.Blogs.Count(blog =>
                    blog.Status ==
                        BlogStatus.Published &&
                    blog.PublishedAt.HasValue &&
                    blog.PublishedAt <= now)
            })
            .ToListAsync(cancellationToken);
    }

    private static PublicBlogCardDto MapCard(
        Blog blog) =>
        new()
        {
            Id = blog.Id,
            CategoryId = blog.CategoryId,
            CategoryName = blog.Category.Name,
            CategorySlug = blog.Category.Slug,
            Title = blog.Title,
            Slug = blog.Slug,
            Summary = blog.Summary,
            ThumbnailUrl = blog.ThumbnailUrl,
            PublishedAt = blog.PublishedAt,
            ViewCount = blog.ViewCount,
            IsFeatured = blog.IsFeatured,
            Tags = blog.BlogTagMappings
                .OrderBy(x => x.Tag.Name)
                .Select(x =>
                    new PublicBlogTagDto
                    {
                        Id = x.Tag.Id,
                        Name = x.Tag.Name,
                        Slug = x.Tag.Slug
                    })
                .ToArray()
        };

    private static PublicBlogDetailDto MapDetail(
        Blog blog) =>
        new()
        {
            Id = blog.Id,
            CategoryId = blog.CategoryId,
            CategoryName = blog.Category.Name,
            CategorySlug = blog.Category.Slug,
            Title = blog.Title,
            Slug = blog.Slug,
            Summary = blog.Summary,
            Content = blog.Content,
            ThumbnailUrl = blog.ThumbnailUrl,
            PublishedAt = blog.PublishedAt,
            ViewCount = blog.ViewCount,
            IsFeatured = blog.IsFeatured,
            Tags = blog.BlogTagMappings
                .OrderBy(x => x.Tag.Name)
                .Select(x =>
                    new PublicBlogTagDto
                    {
                        Id = x.Tag.Id,
                        Name = x.Tag.Name,
                        Slug = x.Tag.Slug
                    })
                .ToArray()
        };
}

