using System;
using System.Threading.Tasks;
using TaO10_BackEnd.DTOs.Exams;

namespace TaO10_BackEnd.Interfaces;

public interface IEmailService
{
    Task SendPasswordAsync(string toEmail, string temporaryPassword);
    Task SendOtpAsync(string toEmail, string otp, TimeSpan ttl);
    Task SendExamSecurityReportAsync(string toEmail, ExamSecurityReportEmailDto report);

}
