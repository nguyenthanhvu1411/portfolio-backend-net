namespace Portfolio.Application.Profiles.Models;

public sealed class PublicCvResource
{
    public string FileUrl { get; init; } = string.Empty;
    public string? PhysicalPath { get; init; }
    public string DownloadFileName { get; init; } = "Portfolio-CV";
    public string ContentType { get; init; } = "application/octet-stream";
}

