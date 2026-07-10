namespace Portfolio.Application.Uploads.DTOs;

public sealed class UploadedFileDto
{
    public int Id { get; init; }
    public string OriginalFileName { get; init; } = string.Empty;
    public string StoredFileName { get; init; } = string.Empty;
    public string FileUrl { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public int? UploadedBy { get; init; }
    public DateTime CreatedAt { get; init; }
}
