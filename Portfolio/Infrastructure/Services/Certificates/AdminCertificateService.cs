using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Certificates.DTOs;
using Portfolio.Application.Certificates.Interfaces;
using Portfolio.Application.Common.Files;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Profiles.Interfaces;
using Portfolio.Common.Exceptions;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Services.Certificates;

public sealed class AdminCertificateService : IAdminCertificateService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;

    public AdminCertificateService(ApplicationDbContext dbContext, IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
    }

    public async Task<IReadOnlyList<CertificateDto>> GetAllAsync(
        CertificateFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Certificates.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();
            query = query.Where(x =>
                x.Name.Contains(keyword) ||
                x.Organization.Contains(keyword) ||
                (x.CredentialId != null && x.CredentialId.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Organization))
        {
            var organization = filter.Organization.Trim();
            query = query.Where(x => x.Organization.Contains(organization));
        }

        var entities = await query
            .OrderByDescending(x => x.IssueDate)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return entities.Select(Map).ToArray();
    }

    public async Task<CertificateDto> CreateAsync(
        CertificateCreateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = new Certificate();
        Apply(entity, request);
        _dbContext.Certificates.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var result = Map(entity);
        AddAuditLog(currentUserId, AuditAction.Create, entity.Id, null, result);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<CertificateDto> UpdateAsync(
        int id,
        CertificateUpdateRequest request,
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
        return new OperationResult { Success = true, Message = "Đã xóa mềm chứng chỉ." };
    }

    public async Task<CertificateDto> ToggleActiveAsync(
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

    public async Task<FileUrlResponse> UploadImageAsync(
        int id,
        IFormFile file,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        await AdminFileUploadValidator.ValidateImageAsync(file, 10, cancellationToken);
        var entity = await GetTrackedAsync(id, cancellationToken);
        var oldUrl = entity.ImageUrl;
        var stored = await _fileStorageService.SaveAsync(file, "uploads/certificates", cancellationToken);
        UploadedFile? uploadedFile = null;

        try
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            entity.ImageUrl = stored.FileUrl;
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
                new { ImageUrl = oldUrl },
                new { ImageUrl = stored.FileUrl });

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

    private async Task<Certificate> GetTrackedAsync(int id, CancellationToken cancellationToken) =>
        await _dbContext.Certificates.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
        ?? throw new NotFoundException($"Không tìm thấy chứng chỉ có Id = {id}.");

    private static void Apply(Certificate entity, CertificateCreateRequest request)
    {
        entity.Name = request.Name.Trim();
        entity.Organization = request.Organization.Trim();
        entity.IssueDate = request.IssueDate?.Date;
        entity.ExpiryDate = request.ExpiryDate?.Date;
        entity.CredentialId = TrimToNull(request.CredentialId);
        entity.CredentialUrl = TrimToNull(request.CredentialUrl);
        entity.Description = TrimToNull(request.Description);
        entity.IsActive = request.IsActive;
    }

    private void AddAuditLog(int userId, AuditAction action, int entityId, object? oldValue, object? newValue)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = nameof(Certificate),
            EntityId = entityId.ToString(),
            OldValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static CertificateDto Map(Certificate x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Organization = x.Organization,
        IssueDate = x.IssueDate,
        ExpiryDate = x.ExpiryDate,
        CredentialId = x.CredentialId,
        CredentialUrl = x.CredentialUrl,
        ImageUrl = x.ImageUrl,
        Description = x.Description,
        IsActive = x.IsActive
    };

    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
