using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaO10_BackEnd.Interfaces;

namespace TaO10_BackEnd.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;
    private static readonly HttpClient _httpClient = new HttpClient();

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendPasswordAsync(string toEmail, string temporaryPassword)
    {
        var subject = "TaO10 - Chúc mừng bạn đã đăng kí tài khoản thành công!";
        var body = $"Xin chào,\n\nMật khẩu đăng nhập tạm thời của bạn là: {temporaryPassword}\nVui lòng đăng nhập và đổi mật khẩu ngay.\n\nNếu bạn không yêu cầu mật khẩu này, hãy bỏ qua email này.";
        await SendEmailInternalAsync(toEmail, subject, body);
    }

    public async Task SendOtpAsync(string toEmail, string otp, TimeSpan ttl)
    {
        var subject = "TaO10 - Mã OTP để lấy lại mật khẩu";
        var body = $"Xin chào,\n\nMã OTP của bạn là: {otp}\nMã có hiệu lực trong {ttl.TotalMinutes:F0} phút.\n\nNếu bạn không yêu cầu mã này, hãy bỏ qua email này.";
        await SendEmailInternalAsync(toEmail, subject, body);
    }

    private async Task SendEmailInternalAsync(string toEmail, string subject, string body)
    {
        // Use the API Key from configuration. We will reuse the Password field or a new BrevoApiKey field.
        var apiKey = _config["SmtpSettings:BrevoApiKey"] ?? _config["SmtpSettings:Password"];
        var senderName = _config["SmtpSettings:SenderName"] ?? "TaO10";
        var senderEmail = _config["SmtpSettings:SenderEmail"];

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(senderEmail))
        {
            _logger.LogWarning("Brevo API settings are not configured. Skipping sending email to {Email}.", toEmail);
            return;
        }

        try
        {
            var payload = new
            {
                sender = new { name = senderName, email = senderEmail },
                to = new[] { new { email = toEmail } },
                subject = subject,
                textContent = body
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
            request.Headers.Add("api-key", apiKey);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email '{Subject}' sent to {Email} via Brevo API.", subject, toEmail);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Brevo API failed to send email. Status: {Status}, Response: {Response}", response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} via Brevo API.", toEmail);
        }
    }
}
