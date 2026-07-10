using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Profiles.Models;
using Portfolio.Application.Profiles.DTOs;
using Portfolio.Application.Profiles.Interfaces;
using Portfolio.Common.Exceptions;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Services.Profiles;

public sealed class PublicProfileService : IPublicProfileService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public PublicProfileService(
        ApplicationDbContext dbContext,
        IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task<PublicProfileDto> GetAsync(
        CancellationToken cancellationToken = default)
    {
        var profile = await _dbContext.Profiles
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(
                x => x.IsActive,
                cancellationToken)
            ?? throw new NotFoundException(
                "Chưa có hồ sơ cá nhân đang hiển thị.");

        var cvFile = string.IsNullOrWhiteSpace(profile.CvUrl)
            ? null
            : await _dbContext.UploadedFiles
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(
                    x => x.FileUrl == profile.CvUrl,
                    cancellationToken);

        return new PublicProfileDto
        {
            Id = profile.Id,
            FullName = profile.FullName,
            JobTitle = profile.JobTitle,
            ShortBio = profile.ShortBio,
            AboutMe = profile.AboutMe,
            AvatarUrl = profile.AvatarUrl,
            BannerUrl = profile.BannerUrl,
            CvUrl = profile.CvUrl,
            CvFileName = cvFile?.OriginalFileName
                ?? GetFileName(profile.CvUrl),
            CvContentType = cvFile?.ContentType
                ?? GetContentType(profile.CvUrl),
            CvFileSize = cvFile?.FileSize,
            Email = profile.Email,
            Phone = profile.Phone,
            Address = profile.Address,
            GithubUrl = profile.GithubUrl,
            LinkedinUrl = profile.LinkedinUrl,
            FacebookUrl = profile.FacebookUrl
        };
    }

    public async Task<PublicCvResource> GetCvAsync(
        CancellationToken cancellationToken = default)
    {
        var profile = await _dbContext.Profiles
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync(
                x => x.IsActive,
                cancellationToken)
            ?? throw new NotFoundException(
                "Chưa có hồ sơ cá nhân đang hiển thị.");

        if (string.IsNullOrWhiteSpace(profile.CvUrl))
        {
            throw new NotFoundException(
                "Hồ sơ hiện chưa có file CV.");
        }

        var uploadedFile = await _dbContext.UploadedFiles
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(
                x => x.FileUrl == profile.CvUrl,
                cancellationToken);

        var physicalPath = ResolvePhysicalPath(profile.CvUrl);

        if (physicalPath is not null &&
            !File.Exists(physicalPath))
        {
            physicalPath = null;
        }

        return new PublicCvResource
        {
            FileUrl = profile.CvUrl,
            PhysicalPath = physicalPath,
            DownloadFileName =
                uploadedFile?.OriginalFileName
                ?? GetFileName(profile.CvUrl)
                ?? "Portfolio-CV",
            ContentType =
                uploadedFile?.ContentType
                ?? GetContentType(profile.CvUrl)
                ?? "application/octet-stream"
        };
    }

    private string? ResolvePhysicalPath(string fileUrl)
    {
        if (Uri.TryCreate(
                fileUrl,
                UriKind.Absolute,
                out _))
        {
            return null;
        }

        var webRootPath =
            string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot")
                : _environment.WebRootPath;

        var relativePath = Uri.UnescapeDataString(fileUrl)
            .TrimStart('/')
            .Replace('/', Path.DirectorySeparatorChar);

        var webRootFullPath = Path.GetFullPath(webRootPath)
            .TrimEnd(Path.DirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        var physicalPath = Path.GetFullPath(
            Path.Combine(webRootPath, relativePath));

        if (!physicalPath.StartsWith(
                webRootFullPath,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Đường dẫn CV nằm ngoài thư mục lưu trữ cho phép.");
        }

        return physicalPath;
    }

    private static string? GetFileName(string? fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return null;
        }

        var clean = fileUrl
            .Split('?', '#')[0];

        return Uri.UnescapeDataString(
            Path.GetFileName(clean));
    }

    private static string? GetContentType(string? fileUrl)
    {
        var extension = Path.GetExtension(
            fileUrl?.Split('?', '#')[0] ?? string.Empty)
            .ToLowerInvariant();

        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" =>
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => null
        };
    }
}

