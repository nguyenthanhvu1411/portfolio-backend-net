namespace Portfolio.Application.Common.Models;

public sealed class FileUrlResponse
{
    public int FileId { get; init; }
    public string FileUrl { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
}
