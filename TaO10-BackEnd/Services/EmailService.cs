using System;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaO10_BackEnd.DTOs.Exams;
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

    public async Task SendExamSecurityReportAsync(
        string toEmail,
        ExamSecurityReportEmailDto report)
    {
        var subject = report.AutoSubmitted
            ? "TaO10 - Thông báo bài làm bị tự động nộp"
            : "TaO10 - Kết quả bài kiểm tra";

        var events = report.Events ?? new List<ExamSecurityEventDto>();

        var eventLines = events.Count == 0
            ? "Không ghi nhận lần rời màn hình nào."
            : string.Join("\n", events.Select(e =>
                $"• Lần {e.Index}: rời lúc {FormatDate(e.LeftAt)}, quay lại lúc {FormatDate(e.ReturnedAt)}, thời gian {e.DurationSeconds} giây"));

        var securitySummary = report.AltTabCount == 0
            ? "Không ghi nhận vi phạm trong quá trình làm bài."
            : $"Hệ thống ghi nhận {report.AltTabCount} lần rời khỏi màn hình làm bài với tổng thời gian {report.TotalAwaySeconds} giây.";

        var autoSubmitMessage = report.AutoSubmitted
            ? $"⚠️ Bài làm đã bị hệ thống tự động nộp do vượt quá giới hạn {report.Threshold} lần rời màn hình cho phép."
            : string.Empty;

        var body = $"""
Kính gửi Quý Phụ huynh,

Hệ thống TAO10 xin gửi kết quả bài kiểm tra của học sinh.

THÔNG TIN BÀI LÀM

Học sinh: {report.StudentName ?? "Không xác định"}

Bài kiểm tra: {report.ExamTitle ?? "Không xác định"}

Thời gian bắt đầu:
{FormatDate(report.StartedAt)}

Thời gian nộp bài:
{FormatDate(report.CompletedAt)}

Kết quả:
{report.CorrectAnswers?.ToString() ?? "0"}/{report.TotalQuestions?.ToString() ?? "0"} câu đúng

Điểm số:
{report.Score?.ToString("0.##") ?? "0"}/10

THÔNG TIN GIÁM SÁT

{securitySummary}

Số lần rời màn hình:
{report.AltTabCount} lần

Tổng thời gian rời màn hình:
{report.TotalAwaySeconds} giây

{autoSubmitMessage}

Chi tiết các lần rời màn hình:

{eventLines}

Quý Phụ huynh vui lòng theo dõi và nhắc nhở học sinh tuân thủ quy định khi làm bài trực tuyến để kết quả phản ánh đúng năng lực thực tế.

Trân trọng,

Hệ thống luyện thi TAO10
""";

        await SendEmailInternalAsync(toEmail, subject, body);
    }


    private static string FormatDate(DateTime? value)
    {
        return value.HasValue ? value.Value.ToString("yyyy-MM-dd HH:mm:ss 'UTC'") : "Không xác định";
    }

    private async Task SendEmailInternalAsync(string toEmail, string subject, string body)
    {
        var apiKey = _config["SmtpSettings:BrevoApiKey"] ?? _config["SmtpSettings:Password"]; 
        var senderName = _config["SmtpSettings:SenderName"] ?? "TaO10";
        var senderEmail = _config["SmtpSettings:SenderEmail"];

        if (string.IsNullOrWhiteSpace(senderEmail))
        {
            _logger.LogError(
                "Email sender is not configured. Set SmtpSettings:SenderEmail to a Brevo-verified sender before sending to {Email}.",
                toEmail);
            throw new InvalidOperationException("Email sender is not configured. Missing SmtpSettings:SenderEmail.");
        }

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            await SendBrevoEmailAsync(apiKey, senderName, senderEmail, toEmail, subject, body);
            return;
        }

        await SendSmtpEmailAsync(senderName, senderEmail, toEmail, subject, body);
    }

    private async Task SendBrevoEmailAsync(
        string apiKey,
        string senderName,
        string senderEmail,
        string toEmail,
        string subject,
        string body)
    {
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
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Brevo accepted email '{Subject}' to {Email}. Status: {Status}, Response: {Response}",
                    subject,
                    toEmail,
                    response.StatusCode,
                    responseContent);
                return;
            }

            _logger.LogError(
                "Brevo API failed to send email '{Subject}' to {Email}. Status: {Status}, Response: {Response}",
                subject,
                toEmail,
                response.StatusCode,
                responseContent);
            throw new InvalidOperationException(
                $"Brevo API failed to send email. Status: {response.StatusCode}. Response: {responseContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} via Brevo API.", toEmail);
            throw;
        }
    }

    private async Task SendSmtpEmailAsync(
        string senderName,
        string senderEmail,
        string toEmail,
        string subject,
        string body)
    {
        var server = _config["SmtpSettings:Server"];
        var username = _config["SmtpSettings:UserName"] ?? senderEmail;
        var password = _config["SmtpSettings:Password"];
        var portValue = _config["SmtpSettings:Port"];

        if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogError("SMTP settings are not configured. Skipping sending email to {Email}.", toEmail);
            throw new InvalidOperationException("SMTP settings are not configured.");
        }

        var port = int.TryParse(portValue, out var parsedPort) ? parsedPort : 587;
        var smtpPassword = server.Contains("gmail", StringComparison.OrdinalIgnoreCase)
            ? password.Replace(" ", string.Empty)
            : password;

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName, Encoding.UTF8),
                Subject = subject,
                SubjectEncoding = Encoding.UTF8,
                Body = body,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = false
            };
            message.To.Add(new MailAddress(toEmail));

            using var client = new SmtpClient(server, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, smtpPassword)
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Email '{Subject}' sent to {Email} via SMTP server {Server}.", subject, toEmail, server);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email} via SMTP server {Server}.", toEmail, server);
            throw new InvalidOperationException($"SMTP failed to send email: {ex.Message}", ex);
        }
    }
}
