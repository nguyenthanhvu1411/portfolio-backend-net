using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Experiences.DTOs;
using Portfolio.Application.Experiences.Interfaces;
using Portfolio.Common.Exceptions;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Services.Experiences;

public sealed class AdminExperienceService : IAdminExperienceService
{
    private readonly ApplicationDbContext _dbContext;

    public AdminExperienceService(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<ExperienceDto>> GetAllAsync(
        ExperienceFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Experiences.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();
            query = query.Where(x =>
                x.Position.Contains(keyword) ||
                x.Company.Contains(keyword) ||
                (x.Location != null && x.Location.Contains(keyword)) ||
                (x.Technologies != null && x.Technologies.Contains(keyword)));
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == filter.IsActive.Value);
        }

        var entities = await query
            .OrderByDescending(x => x.IsCurrent)
            .ThenByDescending(x => x.StartDate)
            .ThenBy(x => x.DisplayOrder)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

        return entities.Select(Map).ToArray();
    }

    public async Task<ExperienceDto> CreateAsync(
        ExperienceCreateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = new Experience();
        Apply(entity, request);

        _dbContext.Experiences.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = Map(entity);
        AddAuditLog(currentUserId, AuditAction.Create, entity.Id, null, result);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<ExperienceDto> UpdateAsync(
        int id,
        ExperienceUpdateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetTrackedAsync(id, cancellationToken);
        var oldValue = Map(entity);
        Apply(entity, request);

        await _dbContext.SaveChangesAsync(cancellationToken);
        var result = Map(entity);
        AddAuditLog(currentUserId, AuditAction.Update, entity.Id, oldValue, result);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<OperationResult> DeleteAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetTrackedAsync(id, cancellationToken);
        var oldValue = Map(entity);
        entity.IsActive = false;

        await _dbContext.SaveChangesAsync(cancellationToken);
        AddAuditLog(currentUserId, AuditAction.Delete, entity.Id, oldValue, Map(entity));
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new OperationResult { Success = true, Message = "Đã xóa mềm kinh nghiệm làm việc." };
    }

    public async Task<ExperienceDto> ToggleActiveAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = await GetTrackedAsync(id, cancellationToken);
        var oldValue = Map(entity);
        entity.IsActive = !entity.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        var result = Map(entity);
        AddAuditLog(currentUserId, AuditAction.Update, entity.Id, oldValue, result);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    private async Task<Experience> GetTrackedAsync(int id, CancellationToken cancellationToken) =>
        await _dbContext.Experiences.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
        ?? throw new NotFoundException($"Không tìm thấy kinh nghiệm có Id = {id}.");

    private static void Apply(Experience entity, ExperienceCreateRequest request)
    {
        entity.Position = request.Position.Trim();
        entity.Company = request.Company.Trim();
        entity.CompanyLogoUrl = TrimToNull(request.CompanyLogoUrl);
        entity.Location = TrimToNull(request.Location);
        entity.StartDate = request.StartDate.Date;
        entity.IsCurrent = request.IsCurrent;
        entity.EndDate = request.IsCurrent ? null : request.EndDate?.Date;
        entity.Description = TrimToNull(request.Description);
        entity.Technologies = TrimToNull(request.Technologies);
        entity.DisplayOrder = request.DisplayOrder;
        entity.IsActive = request.IsActive;
    }

    private void AddAuditLog(int userId, AuditAction action, int entityId, object? oldValue, object? newValue)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = nameof(Experience),
            EntityId = entityId.ToString(),
            OldValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static ExperienceDto Map(Experience x) => new()
    {
        Id = x.Id,
        Position = x.Position,
        Company = x.Company,
        CompanyLogoUrl = x.CompanyLogoUrl,
        Location = x.Location,
        StartDate = x.StartDate,
        EndDate = x.EndDate,
        IsCurrent = x.IsCurrent,
        Description = x.Description,
        Technologies = x.Technologies,
        DisplayOrder = x.DisplayOrder,
        IsActive = x.IsActive
    };

    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
