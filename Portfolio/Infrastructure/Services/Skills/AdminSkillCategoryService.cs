using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Skills.DTOs;
using Portfolio.Application.Skills.Interfaces;
using Portfolio.Common.Exceptions;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Services.Skills;

public sealed class AdminSkillCategoryService : IAdminSkillCategoryService
{
    private readonly ApplicationDbContext _dbContext;

    public AdminSkillCategoryService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SkillCategoryDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.SkillCategories
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new SkillCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive,
                SkillCount = x.Skills.Count
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<SkillCategoryDto> CreateAsync(
        SkillCategoryCreateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        await EnsureNameIsUniqueAsync(name, null, cancellationToken);

        var category = new SkillCategory
        {
            Name = name,
            Description = TrimToNull(request.Description),
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        _dbContext.SkillCategories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = Map(category, 0);
        AddAuditLog(
            currentUserId,
            AuditAction.Create,
            nameof(SkillCategory),
            category.Id,
            oldValue: null,
            newValue: result);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<SkillCategoryDto> UpdateAsync(
        int id,
        SkillCategoryUpdateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.SkillCategories
            .Include(x => x.Skills)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy nhóm kỹ năng có Id = {id}.");

        var oldValue = Map(category, category.Skills.Count);
        var name = request.Name.Trim();

        await EnsureNameIsUniqueAsync(name, id, cancellationToken);

        category.Name = name;
        category.Description = TrimToNull(request.Description);
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;

        var newValue = Map(category, category.Skills.Count);

        AddAuditLog(
            currentUserId,
            AuditAction.Update,
            nameof(SkillCategory),
            category.Id,
            oldValue,
            newValue);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return newValue;
    }

    public async Task<OperationResult> DeleteAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.SkillCategories
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy nhóm kỹ năng có Id = {id}.");

        var hasSkills = await _dbContext.Skills
            .AnyAsync(x => x.CategoryId == id, cancellationToken);

        if (hasSkills)
        {
            throw new ConflictException(
                "Không thể xóa nhóm kỹ năng đang có kỹ năng. " +
                "Hãy chuyển hoặc xóa các kỹ năng thuộc nhóm trước.");
        }

        var oldValue = Map(category, 0);
        _dbContext.SkillCategories.Remove(category);

        AddAuditLog(
            currentUserId,
            AuditAction.Delete,
            nameof(SkillCategory),
            category.Id,
            oldValue,
            newValue: null);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new OperationResult
        {
            Success = true,
            Message = "Đã xóa nhóm kỹ năng thành công."
        };
    }

    private async Task EnsureNameIsUniqueAsync(
        string name,
        int? excludedId,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.SkillCategories
            .AnyAsync(
                x => x.Name == name &&
                     (!excludedId.HasValue || x.Id != excludedId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ConflictException(
                $"Nhóm kỹ năng '{name}' đã tồn tại.");
        }
    }

    private void AddAuditLog(
        int currentUserId,
        AuditAction action,
        string entityName,
        int entityId,
        object? oldValue,
        object? newValue)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = currentUserId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId.ToString(),
            OldValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static SkillCategoryDto Map(
        SkillCategory category,
        int skillCount)
    {
        return new SkillCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive,
            SkillCount = skillCount
        };
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
