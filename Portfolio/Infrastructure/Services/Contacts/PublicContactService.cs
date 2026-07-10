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
    private readonly IEmailSender _emailSender;
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<PublicContactService> _logger;

    public PublicContactService(
        ApplicationDbContext dbContext,
        IEmailSender emailSender,
        IOptions<EmailOptions> emailOptions,
        ILogger<PublicContactService> logger)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _emailOptions = emailOptions.Value;
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
            var fullName = WebUtility.HtmlEncode(
                contact.FullName);

            var email = WebUtility.HtmlEncode(
                contact.Email);

            var phone = WebUtility.HtmlEncode(
                contact.Phone ?? "Không cung cấp");

            var company = WebUtility.HtmlEncode(
                contact.Company ?? "Không cung cấp");

            var subject = WebUtility.HtmlEncode(
                contact.Subject);

            var message = WebUtility.HtmlEncode(
                    contact.Message)
                .Replace(
                    Environment.NewLine,
                    "<br />");

            var createdAt = contact.CreatedAt
                .ToLocalTime()
                .ToString("dd/MM/yyyy HH:mm");

            var htmlBody = $"""
                <!doctype html>
                <html lang="vi">
                <body style="margin:0;background:#f1f5f9;padding:24px;font-family:Arial,sans-serif;color:#0f172a">
                    <div style="max-width:680px;margin:0 auto;background:#ffffff;border:1px solid #e2e8f0;border-radius:12px;overflow:hidden">
                        <div style="background:#2563eb;padding:20px 24px;color:#ffffff">
                            <h1 style="margin:0;font-size:22px">
                                Tin nhắn mới từ Portfolio
                            </h1>
                        </div>

                        <div style="padding:24px">
                            <p style="margin-top:0;color:#475569">
                                Có một người vừa gửi tin nhắn qua form liên hệ trên website.
                            </p>

                            <table style="width:100%;border-collapse:collapse">
                                <tr>
                                    <td style="padding:10px 0;font-weight:bold;width:150px">Họ tên</td>
                                    <td style="padding:10px 0">{fullName}</td>
                                </tr>
                                <tr>
                                    <td style="padding:10px 0;font-weight:bold">Email</td>
                                    <td style="padding:10px 0">
                                        <a href="mailto:{email}">{email}</a>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding:10px 0;font-weight:bold">Điện thoại</td>
                                    <td style="padding:10px 0">{phone}</td>
                                </tr>
                                <tr>
                                    <td style="padding:10px 0;font-weight:bold">Công ty</td>
                                    <td style="padding:10px 0">{company}</td>
                                </tr>
                                <tr>
                                    <td style="padding:10px 0;font-weight:bold">Thời gian</td>
                                    <td style="padding:10px 0">{createdAt}</td>
                                </tr>
                            </table>

                            <div style="margin-top:20px;border-top:1px solid #e2e8f0;padding-top:20px">
                                <h2 style="font-size:17px;margin:0 0 10px">
                                    {subject}
                                </h2>

                                <div style="line-height:1.7;color:#334155">
                                    {message}
                                </div>
                            </div>

                            <p style="margin:24px 0 0;color:#64748b;font-size:13px">
                                Bạn có thể bấm Trả lời để phản hồi trực tiếp đến {email}.
                            </p>
                        </div>
                    </div>
                </body>
                </html>
                """;

            var textBody = $"""
                TIN NHẮN MỚI TỪ PORTFOLIO

                Họ tên: {contact.FullName}
                Email: {contact.Email}
                Điện thoại: {contact.Phone ?? "Không cung cấp"}
                Công ty: {contact.Company ?? "Không cung cấp"}
                Thời gian: {createdAt}

                Tiêu đề:
                {contact.Subject}

                Nội dung:
                {contact.Message}
                """;

            await _emailSender.SendAsync(
                new EmailMessage(
                    To: _emailOptions.NotificationRecipient,
                    Subject:
                        $"[Portfolio] {contact.Subject}",
                    HtmlBody: htmlBody,
                    TextBody: textBody,
                    ReplyTo: contact.Email),
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
