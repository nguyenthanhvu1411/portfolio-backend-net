using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Blogs.DTOs;
using Portfolio.Application.Blogs.Interfaces;
using Portfolio.Application.Common.Models;
using Portfolio.Common.Exceptions;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Services.Blogs;

public sealed class AdminBlogCategoryService : IAdminBlogCategoryService
{
    private readonly ApplicationDbContext _dbContext;

    public AdminBlogCategoryService(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<BlogCategoryDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.BlogCategories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new BlogCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                IsActive = x.IsActive,
                BlogCount = x.Blogs.Count
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<BlogCategoryDto> CreateAsync(
        BlogCategoryCreateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugGenerator.Normalize(name)
            : SlugGenerator.Normalize(request.Slug);

        ValidateSlug(slug);
        await EnsureUniqueAsync(name, slug, null, cancellationToken);

        var entity = new BlogCategory
        {
            Name = name,
            Slug = slug,
            Description = TrimToNull(request.Description),
            IsActive = request.IsActive
        };

        _dbContext.BlogCategories.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = Map(entity, 0);
        AddAuditLog(currentUserId, AuditAction.Create, entity.Id, null, result);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<BlogCategoryDto> UpdateAsync(
        int id,
        BlogCategoryUpdateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.BlogCategories.SingleOrDefaultAsync(
            x => x.Id == id,
            cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy danh mục blog có Id = {id}.");

        var name = request.Name.Trim();
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugGenerator.Normalize(name)
            : SlugGenerator.Normalize(request.Slug);

        ValidateSlug(slug);
        await EnsureUniqueAsync(name, slug, id, cancellationToken);

        var blogCount = await _dbContext.Blogs.CountAsync(
            x => x.CategoryId == id,
            cancellationToken);

        var oldValue = Map(entity, blogCount);
        entity.Name = name;
        entity.Slug = slug;
        entity.Description = TrimToNull(request.Description);
        entity.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = Map(entity, blogCount);
        AddAuditLog(currentUserId, AuditAction.Update, entity.Id, oldValue, result);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<OperationResult> DeleteAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.BlogCategories.SingleOrDefaultAsync(
            x => x.Id == id,
            cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy danh mục blog có Id = {id}.");

        var blogCount = await _dbContext.Blogs.CountAsync(
            x => x.CategoryId == id,
            cancellationToken);

        if (blogCount > 0)
        {
            throw new ConflictException("Không thể xóa danh mục đang có bài viết.");
        }

        var oldValue = Map(entity, 0);
        _dbContext.BlogCategories.Remove(entity);
        AddAuditLog(currentUserId, AuditAction.Delete, entity.Id, oldValue, null);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new OperationResult { Success = true, Message = "Đã xóa danh mục blog thành công." };
    }

    private async Task EnsureUniqueAsync(
        string name,
        string slug,
        int? excludeId,
        CancellationToken cancellationToken)
    {
        if (await _dbContext.BlogCategories.AnyAsync(
                x => x.Name == name && (!excludeId.HasValue || x.Id != excludeId.Value),
                cancellationToken))
        {
            throw new ConflictException("Tên danh mục blog đã tồn tại.");
        }

        if (await _dbContext.BlogCategories.AnyAsync(
                x => x.Slug == slug && (!excludeId.HasValue || x.Id != excludeId.Value),
                cancellationToken))
        {
            throw new ConflictException("Slug danh mục blog đã tồn tại.");
        }
    }

    private static void ValidateSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new RequestValidationException("slug", "Không thể tạo slug hợp lệ từ tên danh mục.");
        }
    }

    private void AddAuditLog(int userId, AuditAction action, int entityId, object? oldValue, object? newValue)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = nameof(BlogCategory),
            EntityId = entityId.ToString(),
            OldValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static BlogCategoryDto Map(BlogCategory x, int blogCount) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Slug = x.Slug,
        Description = x.Description,
        IsActive = x.IsActive,
        BlogCount = blogCount
    };

    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
