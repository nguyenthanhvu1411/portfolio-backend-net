using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Education.DTOs;
using Portfolio.Application.Education.Interfaces;
using Portfolio.Common.Exceptions;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;
using EducationEntity = Portfolio.Domain.Entities.Education;

namespace Portfolio.Infrastructure.Services.Education;

public sealed class AdminEducationService : IAdminEducationService
{
    private readonly ApplicationDbContext _dbContext;

    public AdminEducationService(ApplicationDbContext dbContext) => _dbContext = dbContext;

    public async Task<IReadOnlyList<EducationDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.Education
            .AsNoTracking()
            .OrderByDescending(x => x.EndYear)
            .ThenByDescending(x => x.StartYear)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);

        return entities.Select(Map).ToArray();
    }

    public async Task<EducationDto> CreateAsync(
        EducationCreateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = new EducationEntity();
        Apply(entity, request);

        _dbContext.Education.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = Map(entity);
        AddAuditLog(currentUserId, AuditAction.Create, entity.Id, null, result);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<EducationDto> UpdateAsync(
        int id,
        EducationUpdateRequest request,
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

        return new OperationResult { Success = true, Message = "Đã xóa mềm thông tin học vấn." };
    }

    public async Task<EducationDto> ToggleActiveAsync(
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

    private async Task<EducationEntity> GetTrackedAsync(int id, CancellationToken cancellationToken) =>
        await _dbContext.Education.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
        ?? throw new NotFoundException($"Không tìm thấy học vấn có Id = {id}.");

    private static void Apply(EducationEntity entity, EducationCreateRequest request)
    {
        entity.SchoolName = request.SchoolName.Trim();
        entity.Major = request.Major.Trim();
        entity.Degree = TrimToNull(request.Degree);
        entity.StartYear = request.StartYear;
        entity.EndYear = request.EndYear;
        entity.GPA = TrimToNull(request.GPA);
        entity.Description = TrimToNull(request.Description);
        entity.LogoUrl = TrimToNull(request.LogoUrl);
        entity.IsActive = request.IsActive;
    }

    private void AddAuditLog(int userId, AuditAction action, int entityId, object? oldValue, object? newValue)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = nameof(EducationEntity),
            EntityId = entityId.ToString(),
            OldValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static EducationDto Map(EducationEntity x) => new()
    {
        Id = x.Id,
        SchoolName = x.SchoolName,
        Major = x.Major,
        Degree = x.Degree,
        StartYear = x.StartYear,
        EndYear = x.EndYear,
        GPA = x.GPA,
        Description = x.Description,
        LogoUrl = x.LogoUrl,
        IsActive = x.IsActive
    };

    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
