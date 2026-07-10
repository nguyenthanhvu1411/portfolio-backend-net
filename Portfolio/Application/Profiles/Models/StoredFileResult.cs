namespace Portfolio.Application.Profiles.Models;

public sealed record StoredFileResult(
    string OriginalFileName,
    string StoredFileName,
    string FileUrl,
    string ContentType,
    long FileSize);
