using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using Portfolio.Application.Profiles.Models;
using Portfolio.Common.Exceptions;

namespace Portfolio.Application.Profiles.Validators;

public static class ProfileFileValidator
{
    private const long AvatarMaxBytes = 5 * 1024 * 1024;
    private const long BannerMaxBytes = 10 * 1024 * 1024;
    private const long CvMaxBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> ImageExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

    private static readonly HashSet<string> ImageContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp"
        };

    private static readonly HashSet<string> CvExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".webp"
        };

    private static readonly HashSet<string> CvContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "image/jpeg",
            "image/png",
            "image/webp"
        };

    public static async Task ValidateAsync(
        IFormFile file,
        ProfileAssetType assetType,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length <= 0)
        {
            throw new RequestValidationException(
                "file",
                "Vui lòng chọn file cần tải lên.");
        }

        var extension = Path.GetExtension(file.FileName);
        var contentType = file.ContentType?.Trim() ?? string.Empty;

        switch (assetType)
        {
            case ProfileAssetType.Avatar:
                ValidateLength(file, AvatarMaxBytes, "Ảnh đại diện", 5);
                ValidateImageMetadata(extension, contentType);
                await ValidateImageSignatureAsync(file, extension, cancellationToken);
                break;

            case ProfileAssetType.Banner:
                ValidateLength(file, BannerMaxBytes, "Ảnh banner", 10);
                ValidateImageMetadata(extension, contentType);
                await ValidateImageSignatureAsync(file, extension, cancellationToken);
                break;

            case ProfileAssetType.Cv:
                ValidateLength(file, CvMaxBytes, "File CV", 10);
                ValidateCvMetadata(extension, contentType);
                await ValidateCvSignatureAsync(file, extension, cancellationToken);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(assetType));
        }
    }

    private static void ValidateLength(
        IFormFile file,
        long maximumBytes,
        string displayName,
        int maximumMegabytes)
    {
        if (file.Length > maximumBytes)
        {
            throw new RequestValidationException(
                "file",
                $"{displayName} không được vượt quá {maximumMegabytes} MB.");
        }
    }

    private static void ValidateImageMetadata(
        string extension,
        string contentType)
    {
        if (!ImageExtensions.Contains(extension) ||
            !ImageContentTypes.Contains(contentType))
        {
            throw new RequestValidationException(
                "file",
                "Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP.");
        }
    }

    private static void ValidateCvMetadata(
        string extension,
        string contentType)
    {
        if (!CvExtensions.Contains(extension) ||
            !CvContentTypes.Contains(contentType))
        {
            throw new RequestValidationException(
                "file",
                "CV chỉ chấp nhận file PDF, DOC, DOCX hoặc hình ảnh (JPG, PNG, WEBP).");
        }
    }

    private static async Task ValidateImageSignatureAsync(
        IFormFile file,
        string extension,
        CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var header = new byte[12];
        var read = await stream.ReadAsync(header, cancellationToken);

        var valid = extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" =>
                read >= 3 &&
                header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF,

            ".png" =>
                read >= 8 &&
                header[0] == 0x89 &&
                header[1] == 0x50 &&
                header[2] == 0x4E &&
                header[3] == 0x47 &&
                header[4] == 0x0D &&
                header[5] == 0x0A &&
                header[6] == 0x1A &&
                header[7] == 0x0A,

            ".webp" =>
                read >= 12 &&
                header[0] == 0x52 &&
                header[1] == 0x49 &&
                header[2] == 0x46 &&
                header[3] == 0x46 &&
                header[8] == 0x57 &&
                header[9] == 0x45 &&
                header[10] == 0x42 &&
                header[11] == 0x50,

            _ => false
        };

        if (!valid)
        {
            throw new RequestValidationException(
                "file",
                "Nội dung file ảnh không khớp với định dạng đã khai báo.");
        }
    }

    private static async Task ValidateCvSignatureAsync(
        IFormFile file,
        string extension,
        CancellationToken cancellationToken)
    {
        var ext = extension.ToLowerInvariant();
        if (ext == ".pdf")
        {
            await using var stream = file.OpenReadStream();
            var header = new byte[5];
            var read = await stream.ReadAsync(header, cancellationToken);

            var isPdf = read == 5 &&
                        header[0] == 0x25 &&
                        header[1] == 0x50 &&
                        header[2] == 0x44 &&
                        header[3] == 0x46 &&
                        header[4] == 0x2D;

            if (!isPdf)
            {
                throw new RequestValidationException(
                    "file",
                    "Nội dung file không phải là PDF hợp lệ.");
            }

            return;
        }
        
        if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".webp")
        {
            await ValidateImageSignatureAsync(file, extension, cancellationToken);
            return;
        }

        if (ext == ".doc")
        {
            await using var stream = file.OpenReadStream();
            var header = new byte[8];
            var read = await stream.ReadAsync(header, cancellationToken);
            
            var isDoc = read == 8 && 
                        header[0] == 0xD0 && 
                        header[1] == 0xCF && 
                        header[2] == 0x11 && 
                        header[3] == 0xE0 && 
                        header[4] == 0xA1 && 
                        header[5] == 0xB1 && 
                        header[6] == 0x1A && 
                        header[7] == 0xE1;
                        
            if (!isDoc)
            {
                throw new RequestValidationException(
                    "file",
                    "Nội dung file không phải là DOC hợp lệ.");
            }
            
            return;
        }

        try
        {
            await using var stream = file.OpenReadStream();
            using var archive = new ZipArchive(
                stream,
                ZipArchiveMode.Read,
                leaveOpen: false);

            var hasContentTypes = archive.GetEntry("[Content_Types].xml") is not null;
            var hasWordDocument = archive.GetEntry("word/document.xml") is not null;

            if (!hasContentTypes || !hasWordDocument)
            {
                throw new RequestValidationException(
                    "file",
                    "Nội dung file không phải là DOCX hợp lệ.");
            }
        }
        catch (InvalidDataException)
        {
            throw new RequestValidationException(
                "file",
                "Nội dung file không phải là DOCX hợp lệ.");
        }
    }
}
