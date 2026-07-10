using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Profiles.DTOs;
using Portfolio.Application.Profiles.Interfaces;
using Portfolio.Application.Profiles.Models;
using Portfolio.Application.Profiles.Validators;
using Portfolio.Common.Exceptions;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;
using ProfileEntity = Portfolio.Domain.Entities.Profile;

namespace Portfolio.Infrastructure.Services.Profiles;

public sealed class AdminProfileService : IAdminProfileService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;

    public AdminProfileService(
        ApplicationDbContext dbContext,
        IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
    }

    public async Task<ProfileDto> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var profile = await _dbContext.Profiles
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(
                "Chưa có dữ liệu profile trong hệ thống.");

        return Map(profile);
    }

    public async Task<ProfileDto> UpdateAsync(
        ProfileUpdateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        var profile = await GetProfileEntityAsync(cancellationToken);
        var oldValue = JsonSerializer.Serialize(Map(profile));

        profile.FullName = request.FullName.Trim();
        profile.JobTitle = request.JobTitle.Trim();
        profile.ShortBio = TrimToNull(request.ShortBio);
        profile.AboutMe = TrimToNull(request.AboutMe);
        profile.Email = TrimToNull(request.Email)?.ToLowerInvariant();
        profile.Phone = TrimToNull(request.Phone);
        profile.Address = TrimToNull(request.Address);
        profile.GithubUrl = TrimToNull(request.GithubUrl);
        profile.LinkedinUrl = TrimToNull(request.LinkedinUrl);
        profile.FacebookUrl = TrimToNull(request.FacebookUrl);
        profile.IsActive = request.IsActive;

        var result = Map(profile);

        AddAuditLog(
            currentUserId,
            profile.Id,
            oldValue,
            JsonSerializer.Serialize(result),
            "Cập nhật thông tin profile.");

        await _dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    public Task<FileUploadResponse> UploadAvatarAsync(
        IFormFile file,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        return UploadAssetAsync(
            file,
            ProfileAssetType.Avatar,
            "uploads/profile/avatar",
            currentUserId,
            cancellationToken);
    }

    public Task<FileUploadResponse> UploadBannerAsync(
        IFormFile file,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        return UploadAssetAsync(
            file,
            ProfileAssetType.Banner,
            "uploads/profile/banner",
            currentUserId,
            cancellationToken);
    }

    public Task<FileUploadResponse> UploadCvAsync(
        IFormFile file,
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        return UploadAssetAsync(
            file,
            ProfileAssetType.Cv,
            "uploads/profile/cv",
            currentUserId,
            cancellationToken);
    }

    public Task DeleteAvatarAsync(
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        return DeleteAssetAsync(
            ProfileAssetType.Avatar,
            currentUserId,
            cancellationToken);
    }

    public Task DeleteBannerAsync(
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        return DeleteAssetAsync(
            ProfileAssetType.Banner,
            currentUserId,
            cancellationToken);
    }

    public Task DeleteCvAsync(
        int currentUserId,
        CancellationToken cancellationToken = default)
    {
        return DeleteAssetAsync(
            ProfileAssetType.Cv,
            currentUserId,
            cancellationToken);
    }

    private async Task<FileUploadResponse> UploadAssetAsync(
        IFormFile file,
        ProfileAssetType assetType,
        string folder,
        int currentUserId,
        CancellationToken cancellationToken)
    {
        await ProfileFileValidator.ValidateAsync(
            file,
            assetType,
            cancellationToken);

        var profile = await GetProfileEntityAsync(cancellationToken);
        var oldUrl = GetAssetUrl(profile, assetType);

        var storedFile = await _fileStorageService.SaveAsync(
            file,
            folder,
            cancellationToken);

        try
        {
            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            SetAssetUrl(profile, assetType, storedFile.FileUrl);

            _dbContext.UploadedFiles.Add(new UploadedFile
            {
                OriginalFileName = storedFile.OriginalFileName,
                StoredFileName = storedFile.StoredFileName,
                FileUrl = storedFile.FileUrl,
                ContentType = storedFile.ContentType,
                FileSize = storedFile.FileSize,
                UploadedBy = currentUserId,
                CreatedAt = DateTime.UtcNow
            });

            if (!string.IsNullOrWhiteSpace(oldUrl))
            {
                var oldFile = await _dbContext.UploadedFiles
                    .SingleOrDefaultAsync(
                        x => x.FileUrl == oldUrl,
                        cancellationToken);

                if (oldFile is not null)
                {
                    _dbContext.UploadedFiles.Remove(oldFile);
                }
            }

            AddAuditLog(
                currentUserId,
                profile.Id,
                oldUrl,
                storedFile.FileUrl,
                $"Cập nhật {GetAssetDisplayName(assetType)}.");

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await _fileStorageService.DeleteAsync(
                storedFile.FileUrl,
                cancellationToken);
            throw;
        }

        if (!string.IsNullOrWhiteSpace(oldUrl))
        {
            await _fileStorageService.DeleteAsync(
                oldUrl,
                cancellationToken);
        }

        return new FileUploadResponse
        {
            FileUrl = storedFile.FileUrl,
            OriginalFileName = storedFile.OriginalFileName,
            ContentType = storedFile.ContentType,
            FileSize = storedFile.FileSize
        };
    }

    private async Task DeleteAssetAsync(
        ProfileAssetType assetType,
        int currentUserId,
        CancellationToken cancellationToken)
    {
        var profile = await GetProfileEntityAsync(cancellationToken);
        var oldUrl = GetAssetUrl(profile, assetType);

        if (string.IsNullOrWhiteSpace(oldUrl))
        {
            return;
        }

        await using var transaction = await _dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        SetAssetUrl(profile, assetType, null);

        var uploadedFile = await _dbContext.UploadedFiles
            .SingleOrDefaultAsync(
                x => x.FileUrl == oldUrl,
                cancellationToken);

        if (uploadedFile is not null)
        {
            _dbContext.UploadedFiles.Remove(uploadedFile);
        }

        AddAuditLog(
            currentUserId,
            profile.Id,
            oldUrl,
            null,
            $"Xóa {GetAssetDisplayName(assetType)}.");

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await _fileStorageService.DeleteAsync(
            oldUrl,
            cancellationToken);
    }

    private async Task<ProfileEntity> GetProfileEntityAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.Profiles
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(
                "Chưa có dữ liệu profile trong hệ thống.");
    }

    private void AddAuditLog(
        int userId,
        int profileId,
        string? oldValue,
        string? newValue,
        string description)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = AuditAction.Update,
            EntityName = nameof(ProfileEntity),
            EntityId = profileId.ToString(),
            OldValue = oldValue,
            NewValue = JsonSerializer.Serialize(new
            {
                Description = description,
                Value = newValue
            }),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static string? GetAssetUrl(
        ProfileEntity profile,
        ProfileAssetType assetType)
    {
        return assetType switch
        {
            ProfileAssetType.Avatar => profile.AvatarUrl,
            ProfileAssetType.Banner => profile.BannerUrl,
            ProfileAssetType.Cv => profile.CvUrl,
            _ => throw new ArgumentOutOfRangeException(nameof(assetType))
        };
    }

    private static void SetAssetUrl(
        ProfileEntity profile,
        ProfileAssetType assetType,
        string? fileUrl)
    {
        switch (assetType)
        {
            case ProfileAssetType.Avatar:
                profile.AvatarUrl = fileUrl;
                break;
            case ProfileAssetType.Banner:
                profile.BannerUrl = fileUrl;
                break;
            case ProfileAssetType.Cv:
                profile.CvUrl = fileUrl;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(assetType));
        }
    }

    private static string GetAssetDisplayName(ProfileAssetType assetType)
    {
        return assetType switch
        {
            ProfileAssetType.Avatar => "ảnh đại diện",
            ProfileAssetType.Banner => "ảnh banner",
            ProfileAssetType.Cv => "file CV",
            _ => throw new ArgumentOutOfRangeException(nameof(assetType))
        };
    }

    private static ProfileDto Map(ProfileEntity profile)
    {
        return new ProfileDto
        {
            Id = profile.Id,
            FullName = profile.FullName,
            JobTitle = profile.JobTitle,
            ShortBio = profile.ShortBio,
            AboutMe = profile.AboutMe,
            AvatarUrl = profile.AvatarUrl,
            BannerUrl = profile.BannerUrl,
            CvUrl = profile.CvUrl,
            Email = profile.Email,
            Phone = profile.Phone,
            Address = profile.Address,
            GithubUrl = profile.GithubUrl,
            LinkedinUrl = profile.LinkedinUrl,
            FacebookUrl = profile.FacebookUrl,
            IsActive = profile.IsActive
        };
    }

    private static string? TrimToNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
