using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.Hubs;
using TaO10_BackEnd.Models;
using Microsoft.Extensions.Logging;
using TaO10_BackEnd.Interfaces;
using TaO10_BackEnd.DTOs.Package;

namespace TaO10_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IPaymentProvider _paymentProvider;
        private readonly ILogger<PaymentsController> _logger;
        private readonly IConfiguration _configuration;

        public PaymentsController(AppDbContext context, IHubContext<NotificationHub> hubContext, IPaymentProvider paymentProvider, ILogger<PaymentsController> logger, IConfiguration configuration)
        {
            _context = context;
            _hubContext = hubContext;
            _paymentProvider = paymentProvider;
            _logger = logger;
            _configuration = configuration;
        }

        // POST: api/Payments/checkout
        [Authorize]
        [HttpPost("checkout")]
        public async Task<ActionResult<CheckoutResponse>> Checkout([FromBody] CheckoutRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var package = await _context.Packages.FindAsync(request.PackageId);
            if (package == null) return NotFound("Gói học tập không tồn tại.");

            if (package.Price <= 0) return BadRequest("Giá gói không hợp lệ.");

            var pendingStatus = await _context.Statuses
            .FirstOrDefaultAsync(s => s.EntityType == "Payment" && s.Code == "PENDING");

            if (pendingStatus == null) return BadRequest("Status PENDING for Payment not configured.");

            // Create payment record first with unique GUID-based transaction code
            var payment = new Payment
            {
                PaymentId = Guid.NewGuid(),
                UserId = userId,
                PackageId = package.PackageId,
                ExpectedAmount = package.Price,
                PaymentMethod = request.PaymentMethod,
                StatusId = pendingStatus.StatusId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Use unix milliseconds as order code (numeric and monotonic)
            var orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            payment.TransactionCode = orderCode.ToString();

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            try
            {
                // Setup items
                var items = new List<PaymentItem> { new PaymentItem(package.Name, 1, package.Price) };

                string returnUrl = _configuration["PayOS:ReturnUrl"] ?? "http://localhost:4200/de-thi?payment=success";
                string cancelUrl = _configuration["PayOS:CancelUrl"] ?? "http://localhost:4200/de-thi?payment=cancel";

                string description = $"Thanh toan {package.Name}";
                if (description.Length > 25)
                {
                    description = description.Substring(0, 25);
                }

                var result = await _paymentProvider.CreatePaymentLinkAsync(orderCode, package.Price, description, items, cancelUrl, returnUrl);

                if (result == null || string.IsNullOrWhiteSpace(result.CheckoutUrl))
                {
                    _logger.LogError("Payment provider returned null or invalid result for payment {PaymentId}", payment.PaymentId);
                    return StatusCode(502, "Thanh toán tạm thời không khả dụng. Vui lòng thử lại.");
                }

                return Ok(new CheckoutResponse
                {
                    PaymentId = payment.PaymentId,
                    CheckoutUrl = result.CheckoutUrl,
                    QrCodeUrl = result.QrCode,
                    ExpectedAmount = payment.ExpectedAmount,
                    Message = "Tạo giao dịch thành công. Vui lòng hoàn tất thanh toán."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment link for PaymentId {PaymentId}", payment.PaymentId);
                return StatusCode(502, "Lỗi khi tạo liên kết thanh toán. Vui lòng thử lại sau.");
            }
        }

        // GET: api/Payments/verify-return
        [Authorize]
        [HttpGet("verify-return")]
        public async Task<IActionResult> VerifyReturn([FromQuery] string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode)) return BadRequest("Missing orderCode");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var payment = await _context.Payments
                .Include(p => p.Package)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.TransactionCode == orderCode && p.UserId == userId);

            if (payment == null) return NotFound("Payment not found");

            var successStatus = await _context.Statuses.FirstOrDefaultAsync(s => s.EntityType == "Payment" && s.Code == "SUCCESS");
            var activeUserPkgStatus = await _context.Statuses.FirstOrDefaultAsync(s => s.EntityType == "UserPackage" && s.Code == "ACTIVE");

            if (payment.StatusId == successStatus.StatusId) return Ok(new { message = "Already processed", status = "PAID" });

            // CRITICAL: Check actual payment status with PayOS before marking as success
            if (long.TryParse(orderCode, out var orderCodeLong))
            {
                var paymentStatus = await _paymentProvider.GetPaymentStatusAsync(orderCodeLong);
                
                if (paymentStatus == null)
                {
                    _logger.LogWarning("Could not retrieve payment status from provider for order {OrderCode}", orderCode);
                    return Ok(new { message = "Payment status unknown. Please wait for confirmation.", status = "PENDING" });
                }

                // PayOS SDK enum PaymentLinkStatus returns PascalCase: Paid, Pending, Processing, Cancelled
                var upperStatus = paymentStatus.Status.ToUpper();
                if (upperStatus != "PAID")
                {
                    _logger.LogInformation("Payment for order {OrderCode} has status {Status} — not PAID, skipping activation", orderCode, paymentStatus.Status);
                    
                    // If cancelled or expired, update our record too
                    if (upperStatus == "CANCELLED" || upperStatus == "EXPIRED")
                    {
                        var cancelledStatus = await _context.Statuses.FirstOrDefaultAsync(s => s.EntityType == "Payment" && s.Code == "FAILED");
                        if (cancelledStatus != null && payment.StatusId != cancelledStatus.StatusId)
                        {
                            payment.StatusId = cancelledStatus.StatusId;
                            payment.UpdatedAt = DateTime.UtcNow;
                            await _context.SaveChangesAsync();
                        }
                    }
                    
                    return Ok(new { message = $"Payment is {paymentStatus.Status}. Not yet paid.", status = upperStatus });
                }
            }

            // Only reach here if PayOS confirms PAID
            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                payment.StatusId = successStatus.StatusId;
                payment.ReceivedAmount = payment.ExpectedAmount;
                payment.PaidAt = DateTime.UtcNow;
                payment.UpdatedAt = DateTime.UtcNow;

                var duration = payment.Package?.DurationTime ?? 30;
                var start = DateTime.UtcNow;
                var end = start.AddDays(duration);

                var userPackage = new UserPackage
                {
                    UserPackageId = Guid.NewGuid(),
                    UserId = payment.UserId,
                    PackageId = payment.PackageId,
                    PaymentId = payment.PaymentId,
                    StartDate = start,
                    EndDate = end,
                    StatusId = activeUserPkgStatus.StatusId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.UserPackages.Add(userPackage);

                var notification = new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    UserId = payment.UserId ?? Guid.Empty,
                    Title = "Mua gói thành công!",
                    Content = $"Gói {payment.Package?.Name} của bạn đã được kích hoạt thành công. Hạn dùng đến ngày {end.ToString("dd/MM/yyyy")}.",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Ok(new { message = "Processed successfully", status = "PAID" });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _logger.LogError(ex, "Failed to verify return for payment {PaymentId}", payment.PaymentId);
                return StatusCode(500, "Failed to process return");
            }
        }

        // POST: api/Payments/webhook (Payment provider IPN endpoint)
        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> PaymentWebhook([FromBody] System.Text.Json.JsonElement webhookBody)
        {
            try
            {
                var verified = await _paymentProvider.VerifyWebhookAsync(webhookBody);
                if (verified == null)
                {
                    _logger.LogWarning("Payment webhook verification failed");
                    return BadRequest("Invalid webhook");
                }

                _logger.LogInformation("Webhook received: Code={Code}, OrderCode={OrderCode}, Amount={Amount}",
                    verified.Code, verified.OrderCode, verified.Amount);

                // only process success
                if (verified.Code == "00")
                {
                    var txCode = verified.OrderCode.ToString();
                    _logger.LogInformation("Processing successful payment for transaction: {TxCode}", txCode);

                    var payment = await _context.Payments
                    .Include(p => p.Package)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.TransactionCode == txCode);

                    if (payment == null)
                    {
                        _logger.LogWarning("Payment not found for transaction code {Tx}", txCode);
                        return NotFound("Payment not found");
                    }

                    var successStatus = await _context.Statuses
                    .FirstOrDefaultAsync(s => s.EntityType == "Payment" && s.Code == "SUCCESS");
                    var activeUserPkgStatus = await _context.Statuses
                    .FirstOrDefaultAsync(s => s.EntityType == "UserPackage" && s.Code == "ACTIVE");

                    if (successStatus == null || activeUserPkgStatus == null)
                    {
                        _logger.LogError("Required statuses not configured (Payment:SUCCESS or UserPackage:ACTIVE)");
                        return StatusCode(500, "Server configuration error");
                    }

                    if (payment.StatusId == successStatus.StatusId)
                    {
                        _logger.LogInformation("Payment {PaymentId} already processed", payment.PaymentId);
                        return Ok(new { message = "Already processed" });
                    }

                    if (verified.Amount != payment.ExpectedAmount)
                    {
                        _logger.LogWarning("Webhook amount {Amount} does not match expected {Expected} for payment {PaymentId}", verified.Amount, payment.ExpectedAmount, payment.PaymentId);
                    }

                    using var tx = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        payment.StatusId = successStatus.StatusId;
                        payment.ReceivedAmount = verified.Amount;
                        payment.PaidAt = DateTime.UtcNow;
                        payment.UpdatedAt = DateTime.UtcNow;

                        var duration = payment.Package?.DurationTime ?? 30;
                        var start = DateTime.UtcNow;
                        var end = start.AddDays(duration);

                        var userPackage = new UserPackage
                        {
                            UserPackageId = Guid.NewGuid(),
                            UserId = payment.UserId,
                            PackageId = payment.PackageId,
                            PaymentId = payment.PaymentId,
                            StartDate = start,
                            EndDate = end,
                            StatusId = activeUserPkgStatus.StatusId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.UserPackages.Add(userPackage);

                        var notification = new Notification
                        {
                            NotificationId = Guid.NewGuid(),
                            UserId = payment.UserId ?? Guid.Empty,
                            Title = "Mua gói thành công!",
                            Content = $"Gói {payment.Package?.Name} của bạn đã được kích hoạt thành công. Hạn dùng đến ngày {end.ToString("dd/MM/yyyy")}.",
                            IsRead = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Notifications.Add(notification);

                        await _context.SaveChangesAsync();
                        await tx.CommitAsync();

                        _logger.LogInformation("Payment {PaymentId} processed successfully. UserPackage {UserPackageId} created for User {UserId}",
                            payment.PaymentId, userPackage.UserPackageId, payment.UserId);

                        if (payment.UserId.HasValue)
                        {
                            var userIdStr = payment.UserId.Value.ToString();
                            await _hubContext.Clients.User(userIdStr).SendAsync("ReceiveNotification", notification.Title, notification.Content);
                            await _hubContext.Clients.All.SendAsync("UserPackageUpdated", userIdStr, payment.Package?.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        await tx.RollbackAsync();
                        _logger.LogError(ex, "Failed to process webhook for payment {PaymentId}", payment.PaymentId);
                        return StatusCode(500, "Failed to process webhook");
                    }
                }
                else
                {
                    _logger.LogInformation("Webhook received with non-success code: {Code}", verified.Code);
                }

                return Ok(new { message = "Webhook received successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook processing error");
                return BadRequest($"Webhook error: {ex.Message}");
            }
        }

        // GET: api/Payments/my-history
        [Authorize]
        [HttpGet("my-history")]
        public async Task<ActionResult<IEnumerable<PaymentResponse>>> GetMyHistory()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var history = await _context.Payments
            .Include(p => p.Package)
            .Include(p => p.Status)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentResponse
            {
                PaymentId = p.PaymentId,
                PackageName = p.Package != null ? p.Package.Name : "Gói đã xóa",
                ExpectedAmount = p.ExpectedAmount,
                ReceivedAmount = p.ReceivedAmount,
                PaymentMethod = p.PaymentMethod,
                TransactionCode = p.TransactionCode,
                Status = p.Status.DisplayName ?? p.Status.Code,
                PaidAt = p.PaidAt,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

            return Ok(history);
        }

        // GET: api/Payments/admin/transactions (Admin only)
        [Authorize]
        [HttpGet("admin/transactions")]
        public async Task<ActionResult<IEnumerable<AdminPaymentResponse>>> GetAdminTransactions()
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "admin")
                return Forbid();

            var transactions = await _context.Payments
            .Include(p => p.Package)
            .Include(p => p.User)
            .Include(p => p.Status)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new AdminPaymentResponse
            {
                PaymentId = p.PaymentId,
                PackageName = p.Package != null ? p.Package.Name : "Gói đã xóa",
                ExpectedAmount = p.ExpectedAmount,
                ReceivedAmount = p.ReceivedAmount,
                PaymentMethod = p.PaymentMethod,
                TransactionCode = p.TransactionCode,
                Status = p.Status.DisplayName ?? p.Status.Code,
                PaidAt = p.PaidAt,
                CreatedAt = p.CreatedAt,
                UserEmail = p.User != null ? p.User.Email : "Ẩn danh",
                UserFullName = p.User != null ? p.User.FullName : "Người dùng đã xóa"
            })
            .ToListAsync();

            return Ok(transactions);
        }
    }
}