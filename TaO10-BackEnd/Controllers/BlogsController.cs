using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BlogsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Blogs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var publishedStatus = await _context.Statuses
                .FirstOrDefaultAsync(s => s.EntityType == "BlogPost" && s.Code == "PUBLISHED");

            if (publishedStatus == null) return NotFound("Status 'PUBLISHED' for BlogPost not configured.");

            var blogs = await _context.BlogPosts
                .Where(b => b.StatusId == publishedStatus.StatusId)
                .OrderByDescending(b => b.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(blogs);
        }

        // GET: api/Blogs/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogPost>> GetBlogById(Guid id)
        {
            var blog = await _context.BlogPosts.FindAsync(id);
            if (blog == null) return NotFound("Bài viết không tồn tại.");

            // Increment views count
            blog.ViewsCount = (blog.ViewsCount ?? 0) + 1;
            await _context.SaveChangesAsync();

            return Ok(blog);
        }

        // POST: api/Blogs (Admin only)
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<BlogPost>> CreateBlog([FromBody] BlogPost request)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "admin")
                return Forbid();

            var publishedStatus = await _context.Statuses
                .FirstOrDefaultAsync(s => s.EntityType == "BlogPost" && s.Code == "PUBLISHED");

            if (publishedStatus == null) return BadRequest("Status 'PUBLISHED' for BlogPost not configured.");

            var blog = new BlogPost
            {
                BlogPostId = Guid.NewGuid(),
                Title = request.Title,
                Content = request.Content,
                Meta = request.Meta ?? $"📅 {DateTime.Now.ToString("dd/MM/yyyy")} · 👁 0 lượt xem",
                ViewsCount = 0,
                StatusId = publishedStatus.StatusId,
                CreatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow
            };

            _context.BlogPosts.Add(blog);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBlogById), new { id = blog.BlogPostId }, blog);
        }

        // PUT: api/Blogs/{id} (Admin only)
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBlog(Guid id, [FromBody] BlogPost request)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "admin")
                return Forbid();

            var blog = await _context.BlogPosts.FindAsync(id);
            if (blog == null) return NotFound("Bài viết không tồn tại.");

            blog.Title = request.Title;
            blog.Content = request.Content;
            blog.Meta = request.Meta;
            if (request.StatusId != Guid.Empty)
            {
                blog.StatusId = request.StatusId;
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Blogs/{id} (Admin only)
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlog(Guid id)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "admin")
                return Forbid();

            var blog = await _context.BlogPosts.FindAsync(id);
            if (blog == null) return NotFound("Bài viết không tồn tại.");

            _context.BlogPosts.Remove(blog);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
