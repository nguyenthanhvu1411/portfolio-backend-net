using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Blogs.DTOs;
using Portfolio.Application.Blogs.Interfaces;
using Portfolio.Application.Common.Files;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Profiles.Interfaces;
using Portfolio.Common.Exceptions;
using Portfolio.Domain.Common;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Services.Blogs;

public sealed class AdminBlogService : IAdminBlogService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;

    public AdminBlogService(
        ApplicationDbContext dbContext,
        IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
    }

    public async Task<PagedResult<BlogDto>> GetPagedAsync(
        BlogFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Blogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();
            query = query.Where(x =>
                x.Title.Contains(keyword) ||
                x.Slug.Contains(keyword) ||
                (x.Summary != null && x.Summary.Contains(keyword)) ||
                x.Content.Contains(keyword));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == filter.CategoryId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var entities = await query
            .Include(x => x.Category)
            .Include(x => x.BlogTagMappings)
                .ThenInclude(x => x.Tag)
            .OrderByDescending(x => x.IsFeatured)
            .ThenByDescending(x => x.PublishedAt)
            .ThenByDescending(x => x.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return PagedResult<BlogDto>.Create(
            entities.Select(MapSummary).ToArray(),
            filter.Page,
            filter.PageSize,
            totalCount);
    }

    public async Task<BlogDetailDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return MapDetail(await GetWithRelationsAsync(id, false, cancellationToken));
    }

    public async Task<BlogDto> CreateAsync(
        BlogCreateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        await EnsureActiveCategoryAsync(request.CategoryId, cancellationToken);

        var title = request.Title.Trim();
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? await GenerateUniqueSlugAsync(title, null, cancellationToken)
            : SlugGenerator.Normalize(request.Slug);

        ValidateSlug(slug);
        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            await EnsureSlugUniqueAsync(slug, null, cancellationToken);
        }

        var entity = new Blog
        {
            CategoryId = request.CategoryId,
            Title = title,
            Slug = slug,
            Summary = TrimToNull(request.Summary),
            Content = request.Content.Trim(),
            Status = BlogStatus.Draft,
            ViewCount = 0,
            IsFeatured = false
        };

        _dbContext.Blogs.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await SynchronizeTagsAsync(entity, request.Tags, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = await GetSummaryAsync(entity.Id, cancellationToken);
        AddAuditLog(currentUserId, AuditAction.Create, entity.Id, null, result);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<BlogDto> UpdateAsync(
        int id,
        BlogUpdateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetWithRelationsAsync(id, true, cancellationToken);
        await EnsureActiveCategoryAsync(request.CategoryId, cancellationToken);

        var oldValue = MapDetail(entity);
        entity.CategoryId = request.CategoryId;
        entity.Title = request.Title.Trim();
        entity.Summary = TrimToNull(request.Summary);
        entity.Content = request.Content.Trim();

        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            var slug = SlugGenerator.Normalize(request.Slug);
            ValidateSlug(slug);
            await EnsureSlugUniqueAsync(slug, id, cancellationToken);
            entity.Slug = slug;
        }

        entity.IsFeatured = entity.Status == BlogStatus.Published && request.IsFeatured;

        await SynchronizeTagsAsync(entity, request.Tags, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = await GetSummaryAsync(entity.Id, cancellationToken);
        AddAuditLog(currentUserId, AuditAction.Update, entity.Id, oldValue, result);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<OperationResult> DeleteAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetWithRelationsAsync(id, true, cancellationToken);
        var oldValue = MapDetail(entity);

        entity.Status = BlogStatus.Hidden;
        entity.IsFeatured = false;
        entity.PublishedAt = null;

        await _dbContext.SaveChangesAsync(cancellationToken);
        var newValue = await GetSummaryAsync(entity.Id, cancellationToken);
        AddAuditLog(currentUserId, AuditAction.Delete, entity.Id, oldValue, newValue);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new OperationResult
        {
            Success = true,
            Message = "Đã xóa mềm bài viết bằng trạng thái Hidden."
        };
    }

    public async Task<BlogDto> PublishAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetWithRelationsAsync(id, true, cancellationToken);
        await EnsureActiveCategoryAsync(entity.CategoryId, cancellationToken);

        if (string.IsNullOrWhiteSpace(entity.Content))
        {
            throw new ConflictException("Không thể xuất bản bài viết chưa có nội dung.");
        }

        var oldValue = MapDetail(entity);
        entity.Status = BlogStatus.Published;
        entity.PublishedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        var result = await GetSummaryAsync(entity.Id, cancellationToken);
        AddAuditLog(currentUserId, AuditAction.Publish, entity.Id, oldValue, result);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<BlogDto> UnpublishAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetWithRelationsAsync(id, true, cancellationToken);
        var oldValue = MapDetail(entity);

        entity.Status = BlogStatus.Hidden;
        entity.PublishedAt = null;
        entity.IsFeatured = false;

        await _dbContext.SaveChangesAsync(cancellationToken);
        var result = await GetSummaryAsync(entity.Id, cancellationToken);
        AddAuditLog(currentUserId, AuditAction.Update, entity.Id, oldValue, result);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<BlogDto> ToggleFeaturedAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetWithRelationsAsync(id, true, cancellationToken);

        if (entity.Status != BlogStatus.Published)
        {
            throw new ConflictException("Chỉ bài viết đã xuất bản mới được đặt nổi bật.");
        }

        var oldValue = MapDetail(entity);
        entity.IsFeatured = !entity.IsFeatured;

        await _dbContext.SaveChangesAsync(cancellationToken);
        var result = await GetSummaryAsync(entity.Id, cancellationToken);
        AddAuditLog(currentUserId, AuditAction.Update, entity.Id, oldValue, result);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<FileUrlResponse> UploadThumbnailAsync(
        int id,
        IFormFile file,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        await AdminFileUploadValidator.ValidateImageAsync(file, 10, cancellationToken);

        var entity = await _dbContext.Blogs.SingleOrDefaultAsync(
            x => x.Id == id,
            cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy bài viết có Id = {id}.");

        var oldUrl = entity.ThumbnailUrl;
        var stored = await _fileStorageService.SaveAsync(
            file,
            "uploads/blogs/thumbnails",
            cancellationToken);

        UploadedFile? uploadedFile = null;

        try
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            entity.ThumbnailUrl = stored.FileUrl;
            uploadedFile = new UploadedFile
            {
                OriginalFileName = stored.OriginalFileName,
                StoredFileName = stored.StoredFileName,
                FileUrl = stored.FileUrl,
                ContentType = stored.ContentType,
                FileSize = stored.FileSize,
                UploadedBy = currentUserId,
                CreatedAt = DateTime.UtcNow
            };
            _dbContext.UploadedFiles.Add(uploadedFile);

            if (!string.IsNullOrWhiteSpace(oldUrl))
            {
                var oldFile = await _dbContext.UploadedFiles.SingleOrDefaultAsync(
                    x => x.FileUrl == oldUrl,
                    cancellationToken);
                if (oldFile is not null) _dbContext.UploadedFiles.Remove(oldFile);
            }

            AddAuditLog(
                currentUserId,
                AuditAction.Update,
                entity.Id,
                new { ThumbnailUrl = oldUrl },
                new { ThumbnailUrl = stored.FileUrl });

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await _fileStorageService.DeleteAsync(stored.FileUrl, cancellationToken);
            throw;
        }

        if (!string.IsNullOrWhiteSpace(oldUrl))
        {
            await _fileStorageService.DeleteAsync(oldUrl, cancellationToken);
        }

        return new FileUrlResponse
        {
            FileId = uploadedFile!.Id,
            FileUrl = uploadedFile.FileUrl,
            OriginalFileName = uploadedFile.OriginalFileName,
            ContentType = uploadedFile.ContentType,
            FileSize = uploadedFile.FileSize
        };
    }

    private async Task<Blog> GetWithRelationsAsync(
        int id,
        bool asTracking,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Blogs
            .Include(x => x.Category)
            .Include(x => x.BlogTagMappings)
                .ThenInclude(x => x.Tag)
            .AsQueryable();

        if (!asTracking) query = query.AsNoTracking();

        return await query.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy bài viết có Id = {id}.");
    }

    private async Task<BlogDto> GetSummaryAsync(int id, CancellationToken cancellationToken) =>
        MapSummary(await GetWithRelationsAsync(id, false, cancellationToken));

    private async Task SynchronizeTagsAsync(
        Blog blog,
        IReadOnlyCollection<string> requestedTags,
        CancellationToken cancellationToken)
    {
        var names = requestedTags
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var lowerNames = names.Select(x => x.ToLowerInvariant()).ToArray();
        var tags = await _dbContext.BlogTags
            .Where(x => lowerNames.Contains(x.Name.ToLower()))
            .ToListAsync(cancellationToken);

        foreach (var name in names)
        {
            if (tags.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var baseSlug = SlugGenerator.Normalize(name);
            if (string.IsNullOrWhiteSpace(baseSlug))
            {
                throw new RequestValidationException("tags", $"Tag '{name}' không tạo được slug hợp lệ.");
            }

            var slug = baseSlug;
            var suffix = 2;
            while (await _dbContext.BlogTags.AnyAsync(x => x.Slug == slug, cancellationToken))
            {
                slug = $"{baseSlug}-{suffix++}";
            }

            var tag = new BlogTag { Name = name, Slug = slug };
            _dbContext.BlogTags.Add(tag);
            tags.Add(tag);
        }

        var desiredTags = tags
            .Where(tag => names.Any(name =>
                name.Equals(tag.Name, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        var removedMappings = blog.BlogTagMappings
            .Where(mapping => !desiredTags.Any(tag =>
                tag.Name.Equals(mapping.Tag.Name, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        _dbContext.BlogTagMappings.RemoveRange(removedMappings);

        foreach (var removedMapping in removedMappings)
        {
            blog.BlogTagMappings.Remove(removedMapping);
        }

        var existingNames = blog.BlogTagMappings
            .Select(mapping => mapping.Tag.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var tag in desiredTags.Where(tag => !existingNames.Contains(tag.Name)))
        {
            blog.BlogTagMappings.Add(new BlogTagMapping
            {
                BlogId = blog.Id,
                Tag = tag
            });
        }
    }

    private async Task EnsureActiveCategoryAsync(int categoryId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.BlogCategories.AnyAsync(
            x => x.Id == categoryId && x.IsActive,
            cancellationToken);

        if (!exists)
        {
            throw new RequestValidationException(
                "categoryId", "Danh mục blog không tồn tại hoặc đang ngưng hoạt động.");
        }
    }

    private async Task<string> GenerateUniqueSlugAsync(
        string title,
        int? excludeId,
        CancellationToken cancellationToken)
    {
        var baseSlug = SlugGenerator.Normalize(title);
        ValidateSlug(baseSlug);
        var slug = baseSlug;
        var suffix = 2;

        while (await _dbContext.Blogs.AnyAsync(
                   x => x.Slug == slug && (!excludeId.HasValue || x.Id != excludeId.Value),
                   cancellationToken))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }

    private async Task EnsureSlugUniqueAsync(
        string slug,
        int? excludeId,
        CancellationToken cancellationToken)
    {
        if (await _dbContext.Blogs.AnyAsync(
                x => x.Slug == slug && (!excludeId.HasValue || x.Id != excludeId.Value),
                cancellationToken))
        {
            throw new ConflictException("Slug bài viết đã tồn tại.");
        }
    }

    private static void ValidateSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new RequestValidationException("slug", "Không thể tạo slug hợp lệ cho bài viết.");
        }
    }

    private void AddAuditLog(int userId, AuditAction action, int entityId, object? oldValue, object? newValue)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = nameof(Blog),
            EntityId = entityId.ToString(),
            OldValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static BlogDto MapSummary(Blog x) => new()
    {
        Id = x.Id,
        CategoryId = x.CategoryId,
        CategoryName = x.Category.Name,
        Title = x.Title,
        Slug = x.Slug,
        Summary = x.Summary,
        ThumbnailUrl = x.ThumbnailUrl,
        Status = x.Status,
        StatusName = x.Status.GetDisplayName(),
        PublishedAt = x.PublishedAt,
        ViewCount = x.ViewCount,
        IsFeatured = x.IsFeatured,
        Tags = x.BlogTagMappings.Select(m => m.Tag.Name).OrderBy(x => x).ToArray()
    };

    private static BlogDetailDto MapDetail(Blog x) => new()
    {
        Id = x.Id,
        CategoryId = x.CategoryId,
        CategoryName = x.Category.Name,
        Title = x.Title,
        Slug = x.Slug,
        Summary = x.Summary,
        Content = x.Content,
        ThumbnailUrl = x.ThumbnailUrl,
        Status = x.Status,
        StatusName = x.Status.GetDisplayName(),
        PublishedAt = x.PublishedAt,
        ViewCount = x.ViewCount,
        IsFeatured = x.IsFeatured,
        Tags = x.BlogTagMappings.Select(m => m.Tag.Name).OrderBy(x => x).ToArray()
    };

    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
