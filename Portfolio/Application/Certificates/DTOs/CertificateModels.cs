namespace Portfolio.Application.Certificates.DTOs;

public sealed class CertificateFilterRequest
{
    public string? Keyword { get; set; }
    public string? Organization { get; set; }
}

public sealed class CertificateDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Organization { get; init; } = string.Empty;
    public DateTime? IssueDate { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public string? CredentialId { get; init; }
    public string? CredentialUrl { get; init; }
    public string? ImageUrl { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}

public class CertificateCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? CredentialId { get; set; }
    public string? CredentialUrl { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CertificateUpdateRequest : CertificateCreateRequest
{
}
