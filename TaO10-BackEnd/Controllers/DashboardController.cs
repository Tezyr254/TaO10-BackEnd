using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaO10_BackEnd.Common;
using TaO10_BackEnd.DTOs.Dashboard;
using TaO10_BackEnd.Interfaces;

namespace TaO10_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        private bool IsAdmin()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
            return roleClaim != null && string.Equals(roleClaim.Value, "admin", StringComparison.OrdinalIgnoreCase);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetGeneralStats()
        {
            if (!IsAdmin()) return Forbid();

            try
            {
                var stats = await _dashboardService.GetGeneralStatsAsync();
                return Ok(ApiResponse<DashboardStatsDto>.SuccessResponse(stats, "Lấy thống kê thành công", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetGeneralStats");
                return StatusCode(500, ApiResponse<DashboardStatsDto>.ErrorResponse("Lỗi hệ thống khi tải thống kê", "INTERNAL_ERROR", 500));
            }
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetRecentTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            if (!IsAdmin()) return Forbid();

            try
            {
                if (page < 1 || pageSize < 1)
                {
                    return BadRequest(ApiResponse<TransactionPagedResponse>.ErrorResponse(
                        "Trang và số lượng bản ghi phải lớn hơn 0",
                        "INVALID_PAGINATION",
                        400));
                }

                var result = await _dashboardService.GetRecentTransactionsAsync(page, pageSize);
                return Ok(ApiResponse<TransactionPagedResponse>.SuccessResponse(result, "Lấy danh sách giao dịch thành công", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRecentTransactions");
                return StatusCode(500, ApiResponse<TransactionPagedResponse>.ErrorResponse("Lỗi hệ thống khi tải danh sách giao dịch", "INTERNAL_ERROR", 500));
            }
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueAnalytics([FromQuery] string period = "monthly")
        {
            if (!IsAdmin()) return Forbid();

            try
            {
                var result = await _dashboardService.GetRevenueAnalyticsAsync(period);
                // Can't use ApiResponse<List<T>> easily if it expects class constraint, but ApiResponse generic definition doesn't restrict it. Assuming it works.
                return Ok(ApiResponse<List<RevenueDataDto>>.SuccessResponse(result, "Lấy dữ liệu doanh thu thành công", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRevenueAnalytics");
                return StatusCode(500, ApiResponse<List<RevenueDataDto>>.ErrorResponse("Lỗi hệ thống khi tải dữ liệu doanh thu", "INTERNAL_ERROR", 500));
            }
        }

        [HttpGet("packages")]
        public async Task<IActionResult> GetPackageDistribution()
        {
            if (!IsAdmin()) return Forbid();

            try
            {
                var result = await _dashboardService.GetPackageDistributionAsync();
                return Ok(ApiResponse<List<PackageStatDto>>.SuccessResponse(result, "Lấy phân bố gói dịch vụ thành công", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPackageDistribution");
                return StatusCode(500, ApiResponse<List<PackageStatDto>>.ErrorResponse("Lỗi hệ thống khi tải phân bố gói", "INTERNAL_ERROR", 500));
            }
        }
    }
}
