using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Portfolio.Application.Profiles.Interfaces;
using Portfolio.Application.Profiles.Models;

namespace Portfolio.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _webRootPath;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(
        IWebHostEnvironment environment,
        ILogger<LocalFileStorageService> logger)
    {
        _webRootPath = string.IsNullOrWhiteSpace(environment.WebRootPath)
            ? Path.Combine(environment.ContentRootPath, "wwwroot")
            : environment.WebRootPath;

        _logger = logger;
    }

    public async Task<StoredFileResult> SaveAsync(
        IFormFile file,
        string folder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrWhiteSpace(folder);

        var safeOriginalName = Path.GetFileName(file.FileName);
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(safeOriginalName);
        var extension = Path.GetExtension(safeOriginalName).ToLowerInvariant();

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitizedFileName = new string(fileNameWithoutExt.Where(ch => !invalidChars.Contains(ch)).ToArray());
        if (string.IsNullOrWhiteSpace(sanitizedFileName))
        {
            sanitizedFileName = "file";
        }

        var normalizedFolder = folder
            .Replace('\\', '/')
            .Trim('/');

        var physicalFolder = Path.Combine(
            _webRootPath,
            normalizedFolder.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(physicalFolder);

        var storedFileName = $"{sanitizedFileName}{extension}";
        var physicalPath = Path.Combine(physicalFolder, storedFileName);
        int counter = 1;

        while (File.Exists(physicalPath))
        {
            storedFileName = $"{sanitizedFileName} ({counter}){extension}";
            physicalPath = Path.Combine(physicalFolder, storedFileName);
            counter++;
        }

        await using (var output = new FileStream(
                         physicalPath,
                         FileMode.CreateNew,
                         FileAccess.Write,
                         FileShare.None,
                         bufferSize: 81920,
                         useAsync: true))
        {
            await file.CopyToAsync(output, cancellationToken);
        }

        var fileUrl = $"/{normalizedFolder}/{storedFileName}";

        return new StoredFileResult(
            safeOriginalName,
            storedFileName,
            fileUrl,
            file.ContentType,
            file.Length);
    }

    public Task DeleteAsync(
        string? fileUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return Task.CompletedTask;
        }

        try
        {
            var relativePath = Uri.UnescapeDataString(fileUrl)
                .TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar);

            var webRootFullPath = Path.GetFullPath(_webRootPath)
                .TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

            var fileFullPath = Path.GetFullPath(
                Path.Combine(_webRootPath, relativePath));

            if (!fileFullPath.StartsWith(
                    webRootFullPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "Đường dẫn file nằm ngoài thư mục lưu trữ cho phép.");
            }

            if (File.Exists(fileFullPath))
            {
                File.Delete(fileFullPath);
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Không thể xóa file vật lý {FileUrl}.",
                fileUrl);
        }

        return Task.CompletedTask;
    }
}
