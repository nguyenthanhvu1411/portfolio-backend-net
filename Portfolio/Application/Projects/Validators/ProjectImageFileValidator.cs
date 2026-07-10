using Microsoft.AspNetCore.Http;
using Portfolio.Common.Exceptions;

namespace Portfolio.Application.Projects.Validators;

public static class ProjectImageFileValidator
{
    private const long MaximumBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp"
        };

    public static async Task ValidateAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length <= 0)
        {
            throw new RequestValidationException(
                "file",
                "Vui lòng chọn ảnh cần tải lên.");
        }

        if (file.Length > MaximumBytes)
        {
            throw new RequestValidationException(
                "file",
                "Ảnh dự án không được vượt quá 10 MB.");
        }

        var extension = Path.GetExtension(file.FileName);
        var contentType = file.ContentType?.Trim() ?? string.Empty;

        if (!AllowedExtensions.Contains(extension) ||
            !AllowedContentTypes.Contains(contentType))
        {
            throw new RequestValidationException(
                "file",
                "Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP.");
        }

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
}
