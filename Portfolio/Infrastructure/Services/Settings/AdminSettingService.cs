using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Common.Files;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Profiles.Interfaces;
using Portfolio.Application.Settings.DTOs;
using Portfolio.Application.Settings.Interfaces;
using Portfolio.Common.Exceptions;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Services.Settings;

public sealed class AdminSettingService : IAdminSettingService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;

    public AdminSettingService(
        ApplicationDbContext dbContext,
        IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
    }

    public async Task<SettingDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var setting = await _dbContext.Settings
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Chưa có cấu hình website trong hệ thống.");

        return Map(setting);
    }

    public async Task<SettingDto> UpdateAsync(
        SettingUpdateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var setting = await _dbContext.Settings
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var oldValue = setting is null ? null : Map(setting);

        if (setting is null)
        {
            setting = new Setting();
            _dbContext.Settings.Add(setting);
        }

        setting.SiteName = request.SiteName.Trim();
        setting.ThemeColor = TrimToNull(request.ThemeColor);
        setting.SeoTitle = TrimToNull(request.SeoTitle);
        setting.SeoDescription = TrimToNull(request.SeoDescription);
        setting.ContactEmail = TrimToNull(request.ContactEmail)?.ToLowerInvariant();
        setting.FooterText = TrimToNull(request.FooterText);

        await _dbContext.SaveChangesAsync(cancellationToken);
        var result = Map(setting);

        AddAuditLog(currentUserId, setting.Id, oldValue, result);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<FileUrlResponse> UploadLogoAsync(
        IFormFile file,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        await AdminFileUploadValidator.ValidateImageAsync(file, 5, cancellationToken);
        return await UploadAssetAsync(file, true, "uploads/settings/logo", currentUserId, cancellationToken);
    }

    public async Task<FileUrlResponse> UploadFaviconAsync(
        IFormFile file,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        await AdminFileUploadValidator.ValidateFaviconAsync(file, 2, cancellationToken);
        return await UploadAssetAsync(file, false, "uploads/settings/favicon", currentUserId, cancellationToken);
    }

    private async Task<FileUrlResponse> UploadAssetAsync(
        IFormFile file,
        bool isLogo,
        string folder,
        int currentUserId,
        CancellationToken cancellationToken)
    {
        var setting = await _dbContext.Settings
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Chưa có cấu hình website trong hệ thống.");

        var oldUrl = isLogo ? setting.LogoUrl : setting.FaviconUrl;
        var stored = await _fileStorageService.SaveAsync(file, folder, cancellationToken);
        UploadedFile? uploadedFile = null;

        try
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

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

            if (isLogo) setting.LogoUrl = stored.FileUrl;
            else setting.FaviconUrl = stored.FileUrl;

            if (!string.IsNullOrWhiteSpace(oldUrl))
            {
                var oldFile = await _dbContext.UploadedFiles.SingleOrDefaultAsync(
                    x => x.FileUrl == oldUrl,
                    cancellationToken);
                if (oldFile is not null) _dbContext.UploadedFiles.Remove(oldFile);
            }

            AddAuditLog(
                currentUserId,
                setting.Id,
                new { Asset = isLogo ? "Logo" : "Favicon", FileUrl = oldUrl },
                new { Asset = isLogo ? "Logo" : "Favicon", FileUrl = stored.FileUrl });

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

    private void AddAuditLog(int userId, int settingId, object? oldValue, object? newValue)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = oldValue is null ? AuditAction.Create : AuditAction.Update,
            EntityName = nameof(Setting),
            EntityId = settingId.ToString(),
            OldValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static SettingDto Map(Setting x) => new()
    {
        Id = x.Id,
        SiteName = x.SiteName,
        LogoUrl = x.LogoUrl,
        FaviconUrl = x.FaviconUrl,
        ThemeColor = x.ThemeColor,
        SeoTitle = x.SeoTitle,
        SeoDescription = x.SeoDescription,
        ContactEmail = x.ContactEmail,
        FooterText = x.FooterText
    };

    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
