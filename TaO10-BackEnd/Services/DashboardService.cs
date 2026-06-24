using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.DTOs.Dashboard;
using TaO10_BackEnd.Interfaces;
using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetGeneralStatsAsync()
        {
            var totalCustomers = await _context.Users.CountAsync(u => u.Role == "customer");
            var totalPackages = await _context.Packages.CountAsync();
            
            var totalRevenue = await _context.Payments
                .SumAsync(p => (decimal)(p.ReceivedAmount ?? 0));
                
            var activeExams = await _context.Exams.CountAsync(); // Assuming all are active or could add status filter
            var totalAttempts = await _context.UserExamAttempts.CountAsync();

            return new DashboardStatsDto
            {
                TotalCustomers = totalCustomers,
                TotalPackages = totalPackages,
                TotalRevenue = totalRevenue,
                ActiveExams = activeExams,
                TotalAttempts = totalAttempts
            };
        }

        public async Task<TransactionPagedResponse> GetRecentTransactionsAsync(int pageNumber, int pageSize)
        {
            var query = _context.Payments
                .Include(p => p.User)
                .Include(p => p.Status)
                .OrderByDescending(p => p.PaidAt ?? p.UpdatedAt ?? p.CreatedAt);

            var totalCount = await query.CountAsync();

            var payments = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            var items = payments.Select(p => {
                var fullName = p.User != null && !string.IsNullOrWhiteSpace(p.User.FullName) ? p.User.FullName : "Người dùng ẩn danh";
                var email = p.User != null ? p.User.Email : "";
                var displayNameForInitials = p.User != null && !string.IsNullOrWhiteSpace(p.User.FullName) ? p.User.FullName : (p.User?.Email ?? "U");

                var dateUtc = p.PaidAt ?? p.UpdatedAt ?? p.CreatedAt;
                var dateStr = "";
                if (dateUtc.HasValue)
                {
                    var utcTime = DateTime.SpecifyKind(dateUtc.Value, DateTimeKind.Utc);
                    var vnTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, vnTimeZone);
                    dateStr = vnTime.ToString("dd/MM/yyyy HH:mm");
                }

                return new TransactionDto
                {
                    Id = p.TransactionCode ?? p.PaymentId.ToString(),
                    User = fullName,
                    UserEmail = email,
                    Initials = GetInitials(displayNameForInitials),
                    AvatarBg = GetAvatarBg(displayNameForInitials),
                    Date = dateStr,
                    Amount = FormatCurrency(p.ReceivedAmount ?? p.ExpectedAmount),
                    Status = MapPaymentStatus(p.Status?.Code)
                };
            }).ToList();

            return new TransactionPagedResponse
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        public async Task<List<RevenueDataDto>> GetRevenueAnalyticsAsync(string period)
        {
            var data = new List<RevenueDataDto>();
            var vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var currentDateVn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vnTimeZone);

            if (period.ToLower() == "weekly")
            {
                var daysToSubtract = (int)currentDateVn.DayOfWeek - (int)DayOfWeek.Monday;
                if (daysToSubtract < 0) daysToSubtract += 7;
                var startOfWeekVn = currentDateVn.Date.AddDays(-daysToSubtract);
                
                for (int i = 0; i < 7; i++)
                {
                    var dateVn = startOfWeekVn.AddDays(i);
                    var startOfDayUtc = TimeZoneInfo.ConvertTimeToUtc(dateVn, vnTimeZone);
                    var endOfDayUtc = startOfDayUtc.AddDays(1);
                    
                    var sum = await _context.Payments
                        .Where(p => p.CreatedAt != null && p.CreatedAt >= startOfDayUtc && p.CreatedAt < endOfDayUtc)
                        .SumAsync(p => (decimal)(p.ReceivedAmount ?? 0));
                        
                    data.Add(new RevenueDataDto { Month = GetDayOfWeekName(dateVn.DayOfWeek), Value = sum });
                }
            }
            else if (period.ToLower() == "yearly")
            {
                var yearlyData = await _context.Payments
                    .Where(p => p.CreatedAt != null)
                    .Select(p => new { p.CreatedAt, p.ReceivedAmount })
                    .ToListAsync();
                    
                var grouped = yearlyData
                    .GroupBy(p => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(p.CreatedAt!.Value, DateTimeKind.Utc), vnTimeZone).Year)
                    .Select(g => new {
                        Year = g.Key,
                        Sum = g.Sum(p => (decimal)(p.ReceivedAmount ?? 0))
                    })
                    .OrderBy(x => x.Year)
                    .ToList();
                    
                if (!grouped.Any())
                {
                    data.Add(new RevenueDataDto { Month = currentDateVn.Year.ToString(), Value = 0 });
                }
                else 
                {
                    foreach (var item in grouped)
                    {
                        data.Add(new RevenueDataDto { Month = item.Year.ToString(), Value = item.Sum });
                    }
                }
            }
            else // default to monthly
            {
                for (int i = 1; i <= 12; i++)
                {
                    var startOfMonthVn = new DateTime(currentDateVn.Year, i, 1);
                    var startOfMonthUtc = TimeZoneInfo.ConvertTimeToUtc(startOfMonthVn, vnTimeZone);
                    var endOfMonthUtc = startOfMonthUtc.AddMonths(1);
                    
                    var sum = await _context.Payments
                        .Where(p => p.CreatedAt != null && p.CreatedAt >= startOfMonthUtc && p.CreatedAt < endOfMonthUtc)
                        .SumAsync(p => (decimal)(p.ReceivedAmount ?? 0));
                        
                    data.Add(new RevenueDataDto { Month = $"T{i}", Value = sum });
                }
            }

            return data;
        }

        public async Task<List<PackageStatDto>> GetPackageDistributionAsync()
        {
            var colors = new[] { "#4db8ff", "#2a8fd4", "#1a6ba0", "#006194", "#7dd3fc" };
            
            var packages = await _context.Packages
                .Select(p => new
                {
                    p.PackageId,
                    p.Name,
                    Users = p.UserPackages.Count(up => up.Status.Code.ToLower() == "active" || up.Status.Code.ToLower() == "completed" || up.Status.Code.ToLower() == "success"),
                    Price = p.Price
                })
                .ToListAsync();

            var totalUsers = packages.Sum(p => p.Users);
            if (totalUsers == 0) totalUsers = 1; // avoid division by zero

            var result = packages.Select((p, index) => new PackageStatDto
            {
                Name = p.Name,
                Users = p.Users,
                Price = p.Price,
                Percentage = Math.Round((decimal)p.Users / totalUsers * 100, 1),
                Color = colors[index % colors.Length]
            }).ToList();

            return result;
        }

        // Helper methods matching frontend logic for simplicity
        private string GetInitials(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "U";
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
            return (parts[0].Substring(0, 1) + parts[^1].Substring(0, 1)).ToUpper();
        }

        private string GetAvatarBg(string? name)
        {
            var initials = GetInitials(name);
            var code = initials[0] % 5;
            var colors = new[]
            {
                "bg-sky-100 text-sky-700",
                "bg-indigo-100 text-indigo-700",
                "bg-rose-100 text-rose-700",
                "bg-emerald-100 text-emerald-700",
                "bg-amber-100 text-amber-700"
            };
            return colors[code];
        }

        private string FormatCurrency(int amount)
        {
            return $"{amount:N0} VNĐ";
        }

        private string MapPaymentStatus(string? code)
        {
            if (string.IsNullOrWhiteSpace(code)) return "pending";
            code = code.ToLower();
            if (code.Contains("success") || code.Contains("completed")) return "completed";
            if (code.Contains("fail") || code.Contains("cancel")) return "failed";
            return "pending";
        }

        private string GetDayOfWeekName(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => "T2",
                DayOfWeek.Tuesday => "T3",
                DayOfWeek.Wednesday => "T4",
                DayOfWeek.Thursday => "T5",
                DayOfWeek.Friday => "T6",
                DayOfWeek.Saturday => "T7",
                DayOfWeek.Sunday => "CN",
                _ => ""
            };
        }
    }
}
