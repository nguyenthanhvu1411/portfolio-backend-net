using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using Portfolio.Common.Exceptions;

namespace Portfolio.Application.Common.Files;

public static class AdminFileUploadValidator
{
    private static readonly HashSet<string> ImageExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

    private static readonly HashSet<string> ImageContentTypes =
        new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };

    private static readonly HashSet<string> DocumentExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".webp" };

    private static readonly HashSet<string> DocumentContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "image/jpeg",
            "image/png",
            "image/webp"
        };

    public static async Task ValidateImageAsync(
        IFormFile file,
        int maximumMegabytes = 10,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredAndSize(file, maximumMegabytes);

        var extension = Path.GetExtension(file.FileName);
        var contentType = file.ContentType?.Trim() ?? string.Empty;

        if (!ImageExtensions.Contains(extension) ||
            !ImageContentTypes.Contains(contentType))
        {
            throw new RequestValidationException(
                "file", "Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP.");
        }

        await ValidateImageSignatureAsync(file, extension, cancellationToken);
    }

    public static async Task ValidateFaviconAsync(
        IFormFile file,
        int maximumMegabytes = 2,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredAndSize(file, maximumMegabytes);

        var extension = Path.GetExtension(file.FileName);
        var contentType = file.ContentType?.Trim() ?? string.Empty;

        if (!extension.Equals(".ico", StringComparison.OrdinalIgnoreCase))
        {
            await ValidateImageAsync(file, maximumMegabytes, cancellationToken);
            return;
        }

        var validContentType =
            contentType.Equals("image/x-icon", StringComparison.OrdinalIgnoreCase) ||
            contentType.Equals("image/vnd.microsoft.icon", StringComparison.OrdinalIgnoreCase) ||
            contentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase);

        if (!validContentType)
        {
            throw new RequestValidationException("file", "Content-Type của favicon không hợp lệ.");
        }

        await using var stream = file.OpenReadStream();
        var header = new byte[4];
        var read = await stream.ReadAsync(header, cancellationToken);

        if (read != 4 || header[0] != 0 || header[1] != 0 ||
            header[2] != 1 || header[3] != 0)
        {
            throw new RequestValidationException("file", "Nội dung file không phải ICO hợp lệ.");
        }
    }

    public static async Task ValidateDocumentAsync(
        IFormFile file,
        int maximumMegabytes = 15,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredAndSize(file, maximumMegabytes);

        var extension = Path.GetExtension(file.FileName);
        var contentType = file.ContentType?.Trim() ?? string.Empty;

        if (!DocumentExtensions.Contains(extension) ||
            !DocumentContentTypes.Contains(contentType))
        {
            throw new RequestValidationException("file", "Chỉ chấp nhận file PDF, DOC, DOCX hoặc hình ảnh (JPG, PNG, WEBP).");
        }

        var ext = extension.ToLowerInvariant();
        if (ext == ".pdf")
        {
            await ValidatePdfSignatureAsync(file, cancellationToken);
            return;
        }

        if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".webp")
        {
            await ValidateImageSignatureAsync(file, extension, cancellationToken);
            return;
        }

        if (ext == ".doc")
        {
            await ValidateDocSignatureAsync(file, cancellationToken);
            return;
        }

        ValidateDocxSignature(file);
    }

    private static void ValidateRequiredAndSize(IFormFile file, int maximumMegabytes)
    {
        if (file is null || file.Length <= 0)
        {
            throw new RequestValidationException("file", "Vui lòng chọn file cần tải lên.");
        }

        if (file.Length > maximumMegabytes * 1024L * 1024L)
        {
            throw new RequestValidationException(
                "file", $"Dung lượng file không được vượt quá {maximumMegabytes} MB.");
        }

        var fileName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 255)
        {
            throw new RequestValidationException("file", "Tên file không hợp lệ.");
        }
    }

    private static async Task ValidateImageSignatureAsync(
        IFormFile file, string extension, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var header = new byte[12];
        var read = await stream.ReadAsync(header, cancellationToken);

        var valid = extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E &&
                      header[3] == 0x47 && header[4] == 0x0D && header[5] == 0x0A &&
                      header[6] == 0x1A && header[7] == 0x0A,
            ".webp" => read >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 &&
                       header[3] == 0x46 && header[8] == 0x57 && header[9] == 0x45 &&
                       header[10] == 0x42 && header[11] == 0x50,
            _ => false
        };

        if (!valid)
        {
            throw new RequestValidationException(
                "file", "Nội dung file ảnh không khớp với định dạng đã khai báo.");
        }
    }

    private static async Task ValidatePdfSignatureAsync(
        IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var header = new byte[5];
        var read = await stream.ReadAsync(header, cancellationToken);

        if (read != 5 || header[0] != 0x25 || header[1] != 0x50 ||
            header[2] != 0x44 || header[3] != 0x46 || header[4] != 0x2D)
        {
            throw new RequestValidationException("file", "Nội dung file không phải PDF hợp lệ.");
        }
    }

    private static void ValidateDocxSignature(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);

            if (archive.GetEntry("[Content_Types].xml") is null ||
                archive.GetEntry("word/document.xml") is null)
            {
                throw new RequestValidationException("file", "Nội dung file không phải DOCX hợp lệ.");
            }
        }
        catch (InvalidDataException)
        {
            throw new RequestValidationException("file", "Nội dung file không phải DOCX hợp lệ.");
        }
    }

    private static async Task ValidateDocSignatureAsync(IFormFile file, CancellationToken cancellationToken)
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
            throw new RequestValidationException("file", "Nội dung file không phải là DOC hợp lệ.");
        }
    }
}
