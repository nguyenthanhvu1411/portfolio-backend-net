using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Portfolio.Infrastructure.Email;

public sealed class BrevoEmailService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BrevoEmailService> _logger;

    public BrevoEmailService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<BrevoEmailService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendContactNotificationAsync(
        string customerName,
        string customerEmail,
        string? phone,
        string subject,
        string message,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["Brevo:ApiKey"];
        var senderEmail = _configuration["Brevo:SenderEmail"];
        var senderName = _configuration["Brevo:SenderName"];
        var receiverEmail = _configuration["Brevo:ReceiverEmail"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Brevo API Key chưa được cấu hình.");

        if (string.IsNullOrWhiteSpace(senderEmail))
            throw new InvalidOperationException("Brevo SenderEmail chưa được cấu hình.");

        if (string.IsNullOrWhiteSpace(receiverEmail))
            throw new InvalidOperationException("Brevo ReceiverEmail chưa được cấu hình.");

        // Encode dữ liệu người dùng để tránh chèn HTML.
        var safeName = WebUtility.HtmlEncode(customerName);
        var safeEmail = WebUtility.HtmlEncode(customerEmail);
        var safePhone = WebUtility.HtmlEncode(phone ?? "Không cung cấp");
        var safeSubject = WebUtility.HtmlEncode(subject);
        var safeMessage = WebUtility.HtmlEncode(message)
            .Replace("\r\n", "<br />")
            .Replace("\n", "<br />");

        var requestBody = new
        {
            sender = new
            {
                name = senderName ?? "Portfolio Contact",
                email = senderEmail
            },
            to = new[]
            {
                new
                {
                    email = receiverEmail,
                    name = "Nguyen Thanh Vu"
                }
            },
            replyTo = new
            {
                email = customerEmail,
                name = customerName
            },
            subject = $"[Portfolio] {subject}",
            htmlContent = $"""
                <div style="font-family:Arial,sans-serif;line-height:1.6">
                    <h2>Liên hệ mới từ Portfolio</h2>

                    <p><strong>Họ tên:</strong> {safeName}</p>
                    <p><strong>Email:</strong> {safeEmail}</p>
                    <p><strong>Số điện thoại:</strong> {safePhone}</p>
                    <p><strong>Chủ đề:</strong> {safeSubject}</p>

                    <hr />

                    <p><strong>Nội dung:</strong></p>
                    <p>{safeMessage}</p>
                </div>
                """
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.brevo.com/v3/smtp/email");

        request.Headers.Add("api-key", apiKey);
        request.Headers.Add("accept", "application/json");
        request.Content = JsonContent.Create(requestBody);

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Brevo gửi email thất bại. StatusCode: {StatusCode}. Response: {Response}",
                response.StatusCode,
                responseContent);

            throw new InvalidOperationException(
                $"Không gửi được email thông báo. Brevo status: {(int)response.StatusCode}");
        }

        _logger.LogInformation(
            "Đã gửi thông báo liên hệ đến {ReceiverEmail}",
            receiverEmail);
    }
}
