using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.DTOs.Forum;
using TaO10_BackEnd.Hubs;
using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForumController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ForumController(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // GET: api/Forum/categories
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<ForumCategoryResponse>>> GetCategories()
        {
            var list = await _context.ForumCategories
                .OrderBy(c => c.Name)
                .Select(c => new ForumCategoryResponse
                {
                    ForumCategoryId = c.ForumCategoryId,
                    Name = c.Name,
                    Description = c.Description,
                    ThreadsCount = c.ThreadsCount ?? 0,
                    RepliesCount = c.RepliesCount ?? 0,
                    Badge = c.Badge,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/Forum/categories/{categoryId}/threads
        [HttpGet("categories/{categoryId}/threads")]
        public async Task<ActionResult<IEnumerable<ForumThreadResponse>>> GetCategoryThreads(
            Guid categoryId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var activeStatus = await _context.Statuses
                .FirstOrDefaultAsync(s => s.EntityType == "ForumThread" && s.Code == "ACTIVE");

            if (activeStatus == null) return NotFound("Status 'ACTIVE' for ForumThread not configured.");

            var threads = await _context.ForumThreads
                .Include(t => t.User)
                .Include(t => t.Status)
                .Where(t => t.ForumCategoryId == categoryId && t.StatusId == activeStatus.StatusId)
                .OrderByDescending(t => t.IsPinned)
                .ThenByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = threads.Select(t =>
            {
                List<string> parsedTags = new();
                if (!string.IsNullOrEmpty(t.Tags))
                {
                    try { parsedTags = JsonSerializer.Deserialize<List<string>>(t.Tags) ?? new(); } catch { }
                }

                return new ForumThreadResponse
                {
                    ForumThreadId = t.ForumThreadId,
                    Title = t.Title,
                    Content = t.Content,
                    Excerpt = t.Excerpt ?? (t.Content.Length > 150 ? t.Content.Substring(0, 150) + "..." : t.Content),
                    IsPinned = t.IsPinned ?? false,
                    IsHot = t.IsHot ?? false,
                    Tags = parsedTags,
                    ViewsCount = t.ViewsCount ?? 0,
                    RepliesCount = t.RepliesCount ?? 0,
                    Status = t.Status.DisplayName ?? t.Status.Code,
                    AuthorName = t.User?.FullName ?? "Học sinh ẩn danh",
                    AuthorId = t.UserId ?? Guid.Empty,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                };
            }).ToList();

            return Ok(response);
        }

        // GET: api/Forum/threads/{threadId}
        [HttpGet("threads/{threadId}")]
        public async Task<ActionResult<ForumThreadResponse>> GetThreadDetails(Guid threadId)
        {
            var thread = await _context.ForumThreads
                .Include(t => t.User)
                .Include(t => t.Status)
                .FirstOrDefaultAsync(t => t.ForumThreadId == threadId);

            if (thread == null) return NotFound("Bài viết thảo luận không tồn tại.");

            // Increment views count
            thread.ViewsCount = (thread.ViewsCount ?? 0) + 1;
            await _context.SaveChangesAsync();

            List<string> parsedTags = new();
            if (!string.IsNullOrEmpty(thread.Tags))
            {
                try { parsedTags = JsonSerializer.Deserialize<List<string>>(thread.Tags) ?? new(); } catch { }
            }

            return Ok(new ForumThreadResponse
            {
                ForumThreadId = thread.ForumThreadId,
                Title = thread.Title,
                Content = thread.Content,
                Excerpt = thread.Excerpt,
                IsPinned = thread.IsPinned ?? false,
                IsHot = thread.IsHot ?? false,
                Tags = parsedTags,
                ViewsCount = thread.ViewsCount ?? 0,
                RepliesCount = thread.RepliesCount ?? 0,
                Status = thread.Status.DisplayName ?? thread.Status.Code,
                AuthorName = thread.User?.FullName ?? "Học sinh ẩn danh",
                AuthorId = thread.UserId ?? Guid.Empty,
                CreatedAt = thread.CreatedAt,
                UpdatedAt = thread.UpdatedAt
            });
        }

        // POST: api/Forum/threads
        [Authorize]
        [HttpPost("threads")]
        public async Task<ActionResult<ForumThreadResponse>> CreateThread([FromBody] CreateThreadRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var category = await _context.ForumCategories.FindAsync(request.ForumCategoryId);
            if (category == null) return BadRequest("Chuyên mục không tồn tại.");

            var activeStatus = await _context.Statuses
                .FirstOrDefaultAsync(s => s.EntityType == "ForumThread" && s.Code == "ACTIVE");

            if (activeStatus == null) return BadRequest("Status 'ACTIVE' for ForumThread not configured.");

            var excerptText = request.Content.Length > 150 ? request.Content.Substring(0, 150) + "..." : request.Content;

            var thread = new ForumThread
            {
                ForumThreadId = Guid.NewGuid(),
                UserId = userId,
                ForumCategoryId = request.ForumCategoryId,
                Title = request.Title,
                Content = request.Content,
                Excerpt = excerptText,
                IsPinned = false,
                IsHot = false,
                Tags = JsonSerializer.Serialize(request.Tags),
                ViewsCount = 0,
                RepliesCount = 0,
                StatusId = activeStatus.StatusId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ForumThreads.Add(thread);

            // Increment Category Threads Count
            category.ThreadsCount = (category.ThreadsCount ?? 0) + 1;

            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            return CreatedAtAction(nameof(GetThreadDetails), new { threadId = thread.ForumThreadId }, new ForumThreadResponse
            {
                ForumThreadId = thread.ForumThreadId,
                Title = thread.Title,
                Content = thread.Content,
                Excerpt = thread.Excerpt,
                IsPinned = false,
                IsHot = false,
                Tags = request.Tags,
                ViewsCount = 0,
                RepliesCount = 0,
                Status = "Active",
                AuthorName = user?.FullName ?? "Học sinh ẩn danh",
                AuthorId = userId,
                CreatedAt = thread.CreatedAt,
                UpdatedAt = thread.UpdatedAt
            });
        }

        // GET: api/Forum/threads/{threadId}/replies
        [HttpGet("threads/{threadId}/replies")]
        public async Task<ActionResult<IEnumerable<ForumReplyResponse>>> GetThreadReplies(
            Guid threadId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var replies = await _context.ForumReplies
                .Include(r => r.User)
                .Where(r => r.ForumThreadId == threadId)
                .OrderBy(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ForumReplyResponse
                {
                    ForumReplyId = r.ForumReplyId,
                    ForumThreadId = r.ForumThreadId,
                    Content = r.Content,
                    AuthorName = r.User != null ? r.User.FullName : "Người dùng ẩn danh",
                    AuthorId = r.UserId ?? Guid.Empty,
                    AuthorAvatar = r.User != null ? r.User.Avatar : null,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();

            return Ok(replies);
        }

        // POST: api/Forum/threads/{threadId}/replies
        [Authorize]
        [HttpPost("threads/{threadId}/replies")]
        public async Task<ActionResult<ForumReplyResponse>> PostReply(Guid threadId, [FromBody] CreateReplyRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized();

            var thread = await _context.ForumThreads
                .Include(t => t.ForumCategory)
                .FirstOrDefaultAsync(t => t.ForumThreadId == threadId);

            if (thread == null) return NotFound("Chủ đề thảo luận không tồn tại.");

            var reply = new ForumReply
            {
                ForumReplyId = Guid.NewGuid(),
                ForumThreadId = threadId,
                UserId = userId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ForumReplies.Add(reply);

            // Increment thread replies count
            thread.RepliesCount = (thread.RepliesCount ?? 0) + 1;
            
            // Increment category replies count
            if (thread.ForumCategory != null)
            {
                thread.ForumCategory.RepliesCount = (thread.ForumCategory.RepliesCount ?? 0) + 1;
            }

            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            // Real-Time Notification Logic
            if (thread.UserId.HasValue && thread.UserId.Value != userId)
            {
                // Create DB notification for the thread owner
                var notification = new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    UserId = thread.UserId.Value,
                    Title = "Phản hồi mới trong thảo luận",
                    Content = $"{user?.FullName ?? "Ai đó"} đã trả lời chủ đề '{thread.Title}' của bạn.",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Push real-time alert via SignalR to thread owner
                var threadOwnerIdStr = thread.UserId.Value.ToString();
                await _hubContext.Clients.User(threadOwnerIdStr).SendAsync("ReceiveNotification", notification.Title, notification.Content);
            }

            return Ok(new ForumReplyResponse
            {
                ForumReplyId = reply.ForumReplyId,
                ForumThreadId = reply.ForumThreadId,
                Content = reply.Content,
                AuthorName = user?.FullName ?? "Học sinh ẩn danh",
                AuthorId = userId,
                AuthorAvatar = user?.Avatar,
                CreatedAt = reply.CreatedAt,
                UpdatedAt = reply.UpdatedAt
            });
        }
    }
}
