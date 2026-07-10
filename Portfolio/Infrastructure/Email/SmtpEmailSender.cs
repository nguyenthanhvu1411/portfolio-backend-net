using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Portfolio.Application.Common.Email;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace Portfolio.Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(
        IOptions<EmailOptions> options,
        ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(
        EmailMessage email,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation(
                "Email service đang bị tắt. Bỏ qua email đến {Recipient}.",
                email.To);

            return;
        }

        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                _options.FromName,
                _options.FromEmail));

        message.To.Add(
            MailboxAddress.Parse(email.To));

        if (!string.IsNullOrWhiteSpace(email.ReplyTo))
        {
            message.ReplyTo.Add(
                MailboxAddress.Parse(email.ReplyTo));
        }

        message.Subject = email.Subject
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Trim();

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = email.HtmlBody,
            TextBody = email.TextBody
        };

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient
        {
            Timeout = 15_000
        };

        await client.ConnectAsync(
            _options.Host,
            _options.Port,
            SecureSocketOptions.StartTls,
            cancellationToken);

        await client.AuthenticateAsync(
            _options.Username,
            _options.Password,
            cancellationToken);

        await client.SendAsync(
            message,
            cancellationToken);

        await client.DisconnectAsync(
            quit: true,
            cancellationToken);

        _logger.LogInformation(
            "Đã gửi email thông báo liên hệ đến {Recipient}.",
            email.To);
    }
}
