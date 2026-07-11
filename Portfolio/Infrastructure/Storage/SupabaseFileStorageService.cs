using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.Profiles.Interfaces;
using Portfolio.Application.Profiles.Models;

namespace Portfolio.Infrastructure.Storage;

public sealed class SupabaseFileStorageService
    : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
            ".gif",
            ".pdf"
        };

    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/webp",
            "image/gif",
            "application/pdf"
        };

    private readonly HttpClient _httpClient;
    private readonly SupabaseStorageOptions _options;
    private readonly ILogger<SupabaseFileStorageService> _logger;
    private readonly string _baseUrl;

    public SupabaseFileStorageService(
        HttpClient httpClient,
        IOptions<SupabaseStorageOptions> options,
        ILogger<SupabaseFileStorageService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _baseUrl = _options.Url.TrimEnd('/');
    }

    public async Task<StoredFileResult> SaveAsync(
        IFormFile file,
        string folder,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrWhiteSpace(folder);

        ValidateFile(file);

        var originalFileName =
            Path.GetFileName(file.FileName);

        var extension = Path
            .GetExtension(originalFileName)
            .ToLowerInvariant();

        // Không sử dụng tên file người dùng để tránh:
        // - ký tự tiếng Việt
        // - khoảng trắng
        // - trùng tên
        // - path traversal
        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var normalizedFolder =
            NormalizeFolder(folder);

        var objectPath =
            $"{normalizedFolder}/{storedFileName}";

        var encodedObjectPath =
            EncodeObjectPath(objectPath);

        var bucket =
            Uri.EscapeDataString(_options.Bucket);

        var uploadUrl =
            $"{_baseUrl}/storage/v1/object/" +
            $"{bucket}/{encodedObjectPath}";

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            uploadUrl);

        AddAuthenticationHeaders(request);

        // Vì dùng GUID nên không cần ghi đè.
        request.Headers.TryAddWithoutValidation(
            "x-upsert",
            "false");

        await using var inputStream =
            file.OpenReadStream();

        using var content =
            new StreamContent(inputStream);

        var contentType =
            string.IsNullOrWhiteSpace(file.ContentType)
                ? "application/octet-stream"
                : file.ContentType;

        content.Headers.ContentType =
            MediaTypeHeaderValue.Parse(contentType);

        content.Headers.ContentLength =
            file.Length;

        request.Headers.CacheControl =
            new CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromHours(1)
            };

        request.Content = content;

        using var response =
            await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

        var responseBody =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Upload Supabase Storage thất bại. " +
                "Status: {StatusCode}. Response: {Response}",
                response.StatusCode,
                responseBody);

            throw new InvalidOperationException(
                $"Không thể upload file lên Supabase Storage. " +
                $"HTTP {(int)response.StatusCode}.");
        }

        var publicUrl =
            $"{_baseUrl}/storage/v1/object/public/" +
            $"{bucket}/{encodedObjectPath}";

        return new StoredFileResult(
            originalFileName,
            storedFileName,
            publicUrl,
            contentType,
            file.Length);
    }

    public async Task DeleteAsync(
        string? fileUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return;
        }

        var objectPath =
            TryExtractObjectPath(fileUrl);

        // URL cũ thuộc Render hoặc file ngoài Supabase.
        if (string.IsNullOrWhiteSpace(objectPath))
        {
            return;
        }

        var bucket =
            Uri.EscapeDataString(_options.Bucket);

        var deleteUrl =
            $"{_baseUrl}/storage/v1/object/{bucket}";

        var payload = JsonSerializer.Serialize(
            new
            {
                prefixes = new[]
                {
                    objectPath
                }
            });

        using var request =
            new HttpRequestMessage(
                HttpMethod.Delete,
                deleteUrl);

        AddAuthenticationHeaders(request);

        request.Content = new StringContent(
            payload,
            Encoding.UTF8,
            "application/json");

        using var response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.IsSuccessStatusCode ||
            response.StatusCode == HttpStatusCode.NotFound)
        {
            return;
        }

        var responseBody =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        _logger.LogWarning(
            "Không thể xóa file Supabase {FileUrl}. " +
            "Status: {StatusCode}. Response: {Response}",
            fileUrl,
            response.StatusCode,
            responseBody);
    }

    private void AddAuthenticationHeaders(
        HttpRequestMessage request)
    {
        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                _options.ApiKey);

        request.Headers.TryAddWithoutValidation(
            "apikey",
            _options.ApiKey);
    }

    private void ValidateFile(IFormFile file)
    {
        if (file.Length <= 0)
        {
            throw new InvalidOperationException(
                "File tải lên không có dữ liệu.");
        }

        if (file.Length > _options.MaxFileSizeBytes)
        {
            throw new InvalidOperationException(
                $"Dung lượng file vượt quá " +
                $"{_options.MaxFileSizeBytes / 1024 / 1024} MB.");
        }

        var extension = Path
            .GetExtension(file.FileName)
            .ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                $"Định dạng file {extension} không được hỗ trợ.");
        }

        if (!string.IsNullOrWhiteSpace(file.ContentType) &&
            !AllowedContentTypes.Contains(file.ContentType))
        {
            throw new InvalidOperationException(
                $"Content-Type {file.ContentType} " +
                "không được hỗ trợ.");
        }
    }

    private static string NormalizeFolder(
        string folder)
    {
        var segments = folder
            .Replace('\\', '/')
            .Split(
                '/',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

        if (segments.Length == 0 ||
            segments.Any(segment =>
                segment is "." or ".."))
        {
            throw new InvalidOperationException(
                "Thư mục lưu file không hợp lệ.");
        }

        return string.Join('/', segments);
    }

    private static string EncodeObjectPath(
        string objectPath)
    {
        return string.Join(
            '/',
            objectPath
                .Split(
                    '/',
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString));
    }

    private string? TryExtractObjectPath(
        string fileUrl)
    {
        var publicPrefix =
            $"{_baseUrl}/storage/v1/object/public/" +
            $"{_options.Bucket}/";

        var urlWithoutQuery =
            fileUrl.Split('?', 2)[0];

        if (!urlWithoutQuery.StartsWith(
                publicPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var encodedObjectPath =
            urlWithoutQuery[publicPrefix.Length..];

        return Uri.UnescapeDataString(
            encodedObjectPath);
    }
}
