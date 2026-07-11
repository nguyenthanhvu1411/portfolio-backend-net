using System.Net;
using Microsoft.Extensions.Options;
using Portfolio.Application.Common.Email;
using Portfolio.Application.Contacts.DTOs;
using Portfolio.Application.Contacts.Interfaces;
using Portfolio.Domain.Common;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Email;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Services.Contacts;

public sealed class PublicContactService
    : IPublicContactService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly BrevoEmailService _brevoEmailService;
    private readonly ILogger<PublicContactService> _logger;

    public PublicContactService(
        ApplicationDbContext dbContext,
        BrevoEmailService brevoEmailService,
        ILogger<PublicContactService> logger)
    {
        _dbContext = dbContext;
        _brevoEmailService = brevoEmailService;
        _logger = logger;
    }

    public async Task<PublicContactMessageDto> CreateAsync(
        PublicContactMessageCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = new ContactMessage
        {
            FullName = request.FullName.Trim(),

            Email = request.Email
                .Trim()
                .ToLowerInvariant(),

            Phone = TrimToNull(request.Phone),
            Company = TrimToNull(request.Company),

            Subject = request.Subject.Trim(),
            Message = request.Message.Trim(),

            Status = ContactStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ContactMessages.Add(entity);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        await TrySendNotificationAsync(
            entity,
            cancellationToken);

        return new PublicContactMessageDto
        {
            Id = entity.Id,
            FullName = entity.FullName,
            Email = entity.Email,
            Phone = entity.Phone,
            Company = entity.Company,
            Subject = entity.Subject,
            Message = entity.Message,
            Status = entity.Status,
            StatusName = entity.Status.GetDisplayName(),
            CreatedAt = entity.CreatedAt
        };
    }

    private async Task TrySendNotificationAsync(
        ContactMessage contact,
        CancellationToken cancellationToken)
    {
        try
        {
            await _brevoEmailService.SendContactNotificationAsync(
                contact.FullName,
                contact.Email,
                contact.Phone,
                contact.Subject,
                contact.Message,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            /*
             * Tin nhắn đã được lưu trong database.
             * Email lỗi không làm mất Contact Message.
             */
            _logger.LogError(
                exception,
                "Không thể gửi email thông báo cho ContactMessage {ContactMessageId}.",
                contact.Id);
        }
    }

    private static string? TrimToNull(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
