using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Skills.DTOs;
using Portfolio.Application.Skills.Interfaces;
using Portfolio.Common.Exceptions;
using Portfolio.Domain.Common;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Services.Skills;

public sealed class AdminSkillService : IAdminSkillService
{
    private readonly ApplicationDbContext _dbContext;

    public AdminSkillService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<SkillDto>> GetPagedAsync(
        SkillFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Skills
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();
            query = query.Where(x =>
                x.Name.Contains(keyword) ||
                (x.Description != null && x.Description.Contains(keyword)));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == filter.CategoryId.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == filter.IsActive.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new SkillDto
            {
                Id = x.Id,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                Name = x.Name,
                Level = x.Level,
                LevelName = x.Level.HasValue
                    ? x.Level.Value.GetDisplayName()
                    : null,
                IconUrl = x.IconUrl,
                Description = x.Description,
                DisplayOrder = x.DisplayOrder,
                IsFeatured = x.IsFeatured,
                IsActive = x.IsActive,
                ProjectCount = x.ProjectSkills.Count
            })
            .ToListAsync(cancellationToken);

        return PagedResult<SkillDto>.Create(
            items,
            filter.Page,
            filter.PageSize,
            totalCount);
    }

    public async Task<SkillDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Skills
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new SkillDto
            {
                Id = x.Id,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                Name = x.Name,
                Level = x.Level,
                LevelName = x.Level.HasValue
                    ? x.Level.Value.GetDisplayName()
                    : null,
                IconUrl = x.IconUrl,
                Description = x.Description,
                DisplayOrder = x.DisplayOrder,
                IsFeatured = x.IsFeatured,
                IsActive = x.IsActive,
                ProjectCount = x.ProjectSkills.Count
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy kỹ năng có Id = {id}.");
    }

    public async Task<SkillDto> CreateAsync(
        SkillCreateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);

        var name = request.Name.Trim();
        await EnsureNameIsUniqueInCategoryAsync(
            name,
            request.CategoryId,
            excludedId: null,
            cancellationToken);

        var skill = new Skill
        {
            CategoryId = request.CategoryId,
            Name = name,
            Level = request.Level,
            IconUrl = TrimToNull(request.IconUrl),
            Description = TrimToNull(request.Description),
            DisplayOrder = request.DisplayOrder,
            IsFeatured = request.IsFeatured,
            IsActive = request.IsActive
        };

        _dbContext.Skills.Add(skill);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = await GetByIdAsync(skill.Id, cancellationToken);

        AddAuditLog(
            currentUserId,
            AuditAction.Create,
            nameof(Skill),
            skill.Id,
            oldValue: null,
            newValue: result);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<SkillDto> UpdateAsync(
        int id,
        SkillUpdateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var skill = await _dbContext.Skills
            .Include(x => x.Category)
            .Include(x => x.ProjectSkills)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy kỹ năng có Id = {id}.");

        await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);

        var name = request.Name.Trim();
        await EnsureNameIsUniqueInCategoryAsync(
            name,
            request.CategoryId,
            excludedId: id,
            cancellationToken);

        var oldValue = Map(skill);

        skill.CategoryId = request.CategoryId;
        skill.Name = name;
        skill.Level = request.Level;
        skill.IconUrl = TrimToNull(request.IconUrl);
        skill.Description = TrimToNull(request.Description);
        skill.DisplayOrder = request.DisplayOrder;
        skill.IsFeatured = request.IsFeatured;
        skill.IsActive = request.IsActive;

        // Nếu đổi CategoryId, navigation Category cũ không còn dùng để map.
        var categoryName = await _dbContext.SkillCategories
            .Where(x => x.Id == request.CategoryId)
            .Select(x => x.Name)
            .SingleAsync(cancellationToken);

        var newValue = Map(skill, categoryName);

        AddAuditLog(
            currentUserId,
            AuditAction.Update,
            nameof(Skill),
            skill.Id,
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
        var skill = await _dbContext.Skills
            .Include(x => x.Category)
            .Include(x => x.ProjectSkills)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy kỹ năng có Id = {id}.");

        var oldValue = Map(skill);

        if (skill.ProjectSkills.Count > 0)
        {
            skill.IsActive = false;
            skill.IsFeatured = false;

            var newValue = Map(skill);

            AddAuditLog(
                currentUserId,
                AuditAction.Update,
                nameof(Skill),
                skill.Id,
                oldValue,
                newValue);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new OperationResult
            {
                Success = true,
                Message =
                    "Kỹ năng đang được gắn với dự án nên không xóa vật lý. " +
                    "Hệ thống đã chuyển kỹ năng sang trạng thái ngưng hiển thị."
            };
        }

        _dbContext.Skills.Remove(skill);

        AddAuditLog(
            currentUserId,
            AuditAction.Delete,
            nameof(Skill),
            skill.Id,
            oldValue,
            newValue: null);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new OperationResult
        {
            Success = true,
            Message = "Đã xóa kỹ năng thành công."
        };
    }

    public async Task<SkillDto> ToggleActiveAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var skill = await GetTrackedSkillAsync(id, cancellationToken);
        var oldValue = Map(skill);

        skill.IsActive = !skill.IsActive;

        var newValue = Map(skill);
        AddAuditLog(
            currentUserId,
            AuditAction.Update,
            nameof(Skill),
            skill.Id,
            oldValue,
            newValue);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return newValue;
    }

    public async Task<SkillDto> ToggleFeaturedAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var skill = await GetTrackedSkillAsync(id, cancellationToken);
        var oldValue = Map(skill);

        skill.IsFeatured = !skill.IsFeatured;

        var newValue = Map(skill);
        AddAuditLog(
            currentUserId,
            AuditAction.Update,
            nameof(Skill),
            skill.Id,
            oldValue,
            newValue);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return newValue;
    }

    public async Task<OperationResult> ReorderAsync(
        SkillReorderRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var requestedIds = request.Items
            .Select(x => x.Id)
            .ToArray();

        var skills = await _dbContext.Skills
            .Where(x => requestedIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var foundIds = skills.Select(x => x.Id).ToHashSet();
        var missingIds = requestedIds
            .Where(id => !foundIds.Contains(id))
            .Distinct()
            .OrderBy(id => id)
            .ToArray();

        if (missingIds.Length > 0)
        {
            throw new NotFoundException(
                $"Không tìm thấy kỹ năng có Id: {string.Join(", ", missingIds)}.");
        }

        var orderById = request.Items.ToDictionary(
            x => x.Id,
            x => x.DisplayOrder);

        var oldValue = skills
            .OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.DisplayOrder })
            .ToArray();

        foreach (var skill in skills)
        {
            skill.DisplayOrder = orderById[skill.Id];
        }

        var newValue = skills
            .OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.DisplayOrder })
            .ToArray();

        AddAuditLog(
            currentUserId: currentUserId,
            action: AuditAction.Update,
            entityName: nameof(Skill),
            entityId: 0,
            oldValue: oldValue,
            newValue: newValue,
            entityIdText: "reorder");

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new OperationResult
        {
            Success = true,
            Message = "Đã cập nhật thứ tự hiển thị kỹ năng."
        };
    }

    private async Task<Skill> GetTrackedSkillAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Skills
            .Include(x => x.Category)
            .Include(x => x.ProjectSkills)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundException(
                $"Không tìm thấy kỹ năng có Id = {id}.");
    }

    private async Task EnsureCategoryExistsAsync(
        int categoryId,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.SkillCategories
            .AnyAsync(x => x.Id == categoryId, cancellationToken);

        if (!exists)
        {
            throw new NotFoundException(
                $"Không tìm thấy nhóm kỹ năng có Id = {categoryId}.");
        }
    }

    private async Task EnsureNameIsUniqueInCategoryAsync(
        string name,
        int categoryId,
        int? excludedId,
        CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Skills
            .AnyAsync(
                x => x.CategoryId == categoryId &&
                     x.Name == name &&
                     (!excludedId.HasValue || x.Id != excludedId.Value),
                cancellationToken);

        if (exists)
        {
            throw new ConflictException(
                $"Kỹ năng '{name}' đã tồn tại trong nhóm đã chọn.");
        }
    }

    private void AddAuditLog(
        int currentUserId,
        AuditAction action,
        string entityName,
        int entityId,
        object? oldValue,
        object? newValue,
        string? entityIdText = null)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = currentUserId,
            Action = action,
            EntityName = entityName,
            EntityId = entityIdText ?? entityId.ToString(),
            OldValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static SkillDto Map(
        Skill skill,
        string? categoryName = null)
    {
        return new SkillDto
        {
            Id = skill.Id,
            CategoryId = skill.CategoryId,
            CategoryName = categoryName ?? skill.Category.Name,
            Name = skill.Name,
            Level = skill.Level,
            LevelName = skill.Level?.GetDisplayName(),
            IconUrl = skill.IconUrl,
            Description = skill.Description,
            DisplayOrder = skill.DisplayOrder,
            IsFeatured = skill.IsFeatured,
            IsActive = skill.IsActive,
            ProjectCount = skill.ProjectSkills.Count
        };
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
