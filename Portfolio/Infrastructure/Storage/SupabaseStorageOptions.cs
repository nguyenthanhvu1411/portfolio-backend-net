namespace Portfolio.Infrastructure.Storage;

public sealed class SupabaseStorageOptions
{
    public const string SectionName = "SupabaseStorage";

    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Supabase Secret key hoặc legacy service_role key.
    /// Chỉ được cấu hình trong backend.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    public string Bucket { get; set; } = "portfolio-public";

    public long MaxFileSizeBytes { get; set; } =
        10 * 1024 * 1024;
}
