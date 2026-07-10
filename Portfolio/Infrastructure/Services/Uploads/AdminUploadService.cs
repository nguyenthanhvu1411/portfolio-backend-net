using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Common.Files;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Profiles.Interfaces;
using Portfolio.Application.Uploads.DTOs;
using Portfolio.Application.Uploads.Interfaces;
using Portfolio.Common.Exceptions;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Services.Uploads;

public sealed class AdminUploadService : IAdminUploadService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;

    public AdminUploadService(
        ApplicationDbContext dbContext,
        IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
    }

    public async Task<UploadedFileDto> UploadImageAsync(
        IFormFile file,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        await AdminFileUploadValidator.ValidateImageAsync(file, 10, cancellationToken);
        return await SaveAsync(file, "uploads/common/images", currentUserId, cancellationToken);
    }

    public async Task<UploadedFileDto> UploadFileAsync(
        IFormFile file,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        await AdminFileUploadValidator.ValidateDocumentAsync(file, 15, cancellationToken);
        return await SaveAsync(file, "uploads/common/files", currentUserId, cancellationToken);
    }

    public async Task<UploadedFileDto> UploadCvAsync(
        IFormFile file,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        await AdminFileUploadValidator.ValidateDocumentAsync(file, 10, cancellationToken);
        return await SaveAsync(file, "uploads/cv", currentUserId, cancellationToken);
    }

    public async Task<OperationResult> DeleteAsync(
        int fileId,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var file = await _dbContext.UploadedFiles.SingleOrDefaultAsync(
            x => x.Id == fileId,
            cancellationToken)
            ?? throw new NotFoundException($"Không tìm thấy file upload có Id = {fileId}.");

        if (await IsReferencedAsync(file.FileUrl, cancellationToken))
        {
            throw new ConflictException(
                "File đang được sử dụng. Hãy gỡ liên kết khỏi dữ liệu trước khi xóa.");
        }

        var oldValue = Map(file);
        _dbContext.UploadedFiles.Remove(file);

        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = currentUserId,
            Action = AuditAction.Delete,
            EntityName = nameof(UploadedFile),
            EntityId = file.Id.ToString(),
            OldValue = JsonSerializer.Serialize(oldValue),
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _fileStorageService.DeleteAsync(file.FileUrl, cancellationToken);

        return new OperationResult
        {
            Success = true,
            Message = "Đã xóa file upload thành công."
        };
    }

    private async Task<UploadedFileDto> SaveAsync(
        IFormFile file,
        string folder,
        int currentUserId,
        CancellationToken cancellationToken)
    {
        var stored = await _fileStorageService.SaveAsync(file, folder, cancellationToken);

        try
        {
            var entity = new UploadedFile
            {
                OriginalFileName = stored.OriginalFileName,
                StoredFileName = stored.StoredFileName,
                FileUrl = stored.FileUrl,
                ContentType = stored.ContentType,
                FileSize = stored.FileSize,
                UploadedBy = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.UploadedFiles.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var result = Map(entity);
            _dbContext.AuditLogs.Add(new AuditLog
            {
                UserId = currentUserId,
                Action = AuditAction.Create,
                EntityName = nameof(UploadedFile),
                EntityId = entity.Id.ToString(),
                NewValue = JsonSerializer.Serialize(result),
                CreatedAt = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            return result;
        }
        catch
        {
            await _fileStorageService.DeleteAsync(stored.FileUrl, cancellationToken);
            throw;
        }
    }

    private async Task<bool> IsReferencedAsync(string fileUrl, CancellationToken cancellationToken)
    {
        if (await _dbContext.Profiles.AnyAsync(
                x => x.AvatarUrl == fileUrl || x.BannerUrl == fileUrl || x.CvUrl == fileUrl,
                cancellationToken)) return true;

        if (await _dbContext.Projects.AnyAsync(x => x.ThumbnailUrl == fileUrl, cancellationToken)) return true;
        if (await _dbContext.ProjectImages.AnyAsync(x => x.ImageUrl == fileUrl, cancellationToken)) return true;
        if (await _dbContext.Certificates.AnyAsync(x => x.ImageUrl == fileUrl, cancellationToken)) return true;
        if (await _dbContext.Settings.AnyAsync(
                x => x.LogoUrl == fileUrl || x.FaviconUrl == fileUrl,
                cancellationToken)) return true;

        return await _dbContext.Blogs.AnyAsync(x => x.ThumbnailUrl == fileUrl, cancellationToken);
    }

    private static UploadedFileDto Map(UploadedFile x) => new()
    {
        Id = x.Id,
        OriginalFileName = x.OriginalFileName,
        StoredFileName = x.StoredFileName,
        FileUrl = x.FileUrl,
        ContentType = x.ContentType,
        FileSize = x.FileSize,
        UploadedBy = x.UploadedBy,
        CreatedAt = x.CreatedAt
    };
}
