using Portfolio.Domain.Enums;

namespace Portfolio.Application.Contacts.DTOs;

public sealed class PublicContactMessageCreateRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }
    public string? Company { get; set; }

    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    // Honeypot: frontend phải để trống và có thể ẩn bằng CSS.
    public string? Website { get; set; }
}

public sealed class PublicContactMessageDto
{
    public int Id { get; init; }

    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;

    public string? Phone { get; init; }
    public string? Company { get; init; }

    public string Subject { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public ContactStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }
}

