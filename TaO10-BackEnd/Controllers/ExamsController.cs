using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.DTOs.Exam;
using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExamsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExamsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Exams
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExamResponse>>> GetExams(
            [FromQuery] string? level,
            [FromQuery] int? year,
            [FromQuery] string? examType,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var activeExamStatus = await _context.Statuses
                .FirstOrDefaultAsync(s => s.EntityType == "Exam" && s.Code == "ACTIVE");

            if (activeExamStatus == null) return NotFound("Status 'ACTIVE' for Exam not configured.");

            var query = _context.Exams
                .Include(e => e.PackageExams)
                .Include(e => e.Status)
                .Where(e => e.StatusId == activeExamStatus.StatusId);

            // Filters
            if (!string.IsNullOrWhiteSpace(level))
                query = query.Where(e => e.Level == level);

            if (year.HasValue)
                query = query.Where(e => e.Year == year.Value);

            if (!string.IsNullOrWhiteSpace(examType))
                query = query.Where(e => e.ExamType == examType);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(e => e.Title.ToLower().Contains(search.ToLower()));

            // User authorization status (optional progress load)
            Guid? userId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedId))
            {
                userId = parsedId;
            }

            var hasActivePackage = false;
            var unlockedExamIds = new List<Guid>();

            if (userId.HasValue)
            {
                var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
                if (roleClaim != null && roleClaim.Value == "admin")
                {
                    hasActivePackage = true;
                }
                else
                {
                    var activeUserPkgStatus = await _context.Statuses
                        .FirstOrDefaultAsync(s => s.EntityType == "UserPackage" && s.Code == "ACTIVE");

                    var activeUserPackages = await _context.UserPackages
                        .Where(up => up.UserId == userId.Value && 
                                     up.StatusId == (activeUserPkgStatus != null ? activeUserPkgStatus.StatusId : Guid.Empty) &&
                                     (up.EndDate == null || up.EndDate >= DateTime.UtcNow) &&
                                     up.PackageId != null)
                        .Select(up => up.PackageId ?? Guid.Empty)
                        .ToListAsync();

                    if (activeUserPackages.Any())
                    {
                        hasActivePackage = true;
                        unlockedExamIds = await _context.PackageExams
                            .Where(pe => activeUserPackages.Contains(pe.PackageId))
                            .Select(pe => pe.ExamId)
                            .ToListAsync();
                    }
                }
            }

            if (!hasActivePackage)
            {
                return Ok(new List<ExamResponse>());
            }

            var roleClaimCheck = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
            var isAdmin = roleClaimCheck != null && roleClaimCheck.Value == "admin";
            if (!isAdmin)
            {
                query = query.Where(e => unlockedExamIds.Contains(e.ExamId));
            }

            var list = await query
                .OrderBy(e => e.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Load progress if user logged in
            var userProgress = new Dictionary<Guid, int>();
            if (userId.HasValue)
            {
                userProgress = await _context.UserProgresses
                    .Where(up => up.UserId == userId.Value && up.ItemType == "EXAM")
                    .ToDictionaryAsync(up => up.ItemId ?? Guid.Empty, up => up.ProgressPercentage ?? 0);
            }

            var response = list.Select(e => new ExamResponse
            {
                ExamId = e.ExamId,
                Title = e.Title,
                Description = e.Description,
                QuestionsCount = e.QuestionsCount,
                DurationTime = e.DurationTime,
                Level = e.Level,
                Year = e.Year,
                ExamType = e.ExamType,
                ViewsCount = e.ViewsCount ?? 0,
                AttemptsCount = e.AttemptsCount ?? 0,
                Status = e.Status.DisplayName ?? e.Status.Code,
                IsPremium = e.PackageExams.Any(),
                ProgressPercentage = userProgress.ContainsKey(e.ExamId) ? userProgress[e.ExamId] : 0,
                CreatedAt = e.CreatedAt
            }).ToList();

            return Ok(response);
        }

        // GET: api/Exams/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ExamResponse>> GetExamById(Guid id)
        {
            var exam = await _context.Exams
                .Include(e => e.PackageExams)
                .Include(e => e.Status)
                .FirstOrDefaultAsync(e => e.ExamId == id);

            if (exam == null) return NotFound("Đề thi không tồn tại.");

            Guid? userId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var parsedId))
                userId = parsedId;

            int progress = 0;
            if (userId.HasValue)
            {
                var userProg = await _context.UserProgresses
                    .FirstOrDefaultAsync(up => up.UserId == userId.Value && up.ItemType == "EXAM" && up.ItemId == id);
                if (userProg != null) progress = userProg.ProgressPercentage ?? 0;
            }

            return Ok(new ExamResponse
            {
                ExamId = exam.ExamId,
                Title = exam.Title,
                Description = exam.Description,
                QuestionsCount = exam.QuestionsCount,
                DurationTime = exam.DurationTime,
                Level = exam.Level,
                Year = exam.Year,
                ExamType = exam.ExamType,
                ViewsCount = exam.ViewsCount ?? 0,
                AttemptsCount = exam.AttemptsCount ?? 0,
                Status = exam.Status.DisplayName ?? exam.Status.Code,
                IsPremium = exam.PackageExams.Any(),
                ProgressPercentage = progress,
                CreatedAt = exam.CreatedAt
            });
        }

        // POST: api/Exams (Admin only)
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ExamResponse>> CreateExam([FromBody] CreateExamRequest request)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "admin")
                return Forbid();

            var activeStatus = await _context.Statuses
                .FirstOrDefaultAsync(s => s.EntityType == "Exam" && s.Code == "ACTIVE");

            if (activeStatus == null) return BadRequest("Status 'ACTIVE' for Exam not configured.");

            var exam = new Exam
            {
                ExamId = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                QuestionsCount = 0,
                DurationTime = request.DurationTime,
                Level = request.Level,
                Year = request.Year,
                ExamType = request.ExamType,
                ViewsCount = 0,
                AttemptsCount = 0,
                StatusId = activeStatus.StatusId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();

            // If premium, link with Premium Package
            if (request.IsPremium)
            {
                var premiumPkg = await _context.Packages.FirstOrDefaultAsync(p => p.Name.Contains("Premium"));
                if (premiumPkg != null)
                {
                    var packageExam = new PackageExam
                    {
                        PackageExamId = Guid.NewGuid(),
                        PackageId = premiumPkg.PackageId,
                        ExamId = exam.ExamId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.PackageExams.Add(packageExam);
                    await _context.SaveChangesAsync();
                }
            }

            return CreatedAtAction(nameof(GetExamById), new { id = exam.ExamId }, new ExamResponse
            {
                ExamId = exam.ExamId,
                Title = exam.Title,
                Description = exam.Description,
                QuestionsCount = 0,
                DurationTime = exam.DurationTime,
                Level = exam.Level,
                Year = exam.Year,
                ExamType = exam.ExamType,
                Status = "Active",
                IsPremium = request.IsPremium,
                CreatedAt = exam.CreatedAt
            });
        }

        // PUT: api/Exams/{id} (Admin only)
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExam(Guid id, [FromBody] UpdateExamRequest request)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "admin")
                return Forbid();

            var exam = await _context.Exams.FindAsync(id);
            if (exam == null) return NotFound("Đề thi không tồn tại.");

            var status = await _context.Statuses
                .FirstOrDefaultAsync(s => s.EntityType == "Exam" && s.Code == request.Status);

            if (status == null) return BadRequest("Trạng thái không hợp lệ.");

            exam.Title = request.Title;
            exam.Description = request.Description;
            exam.DurationTime = request.DurationTime;
            exam.Level = request.Level;
            exam.Year = request.Year;
            exam.ExamType = request.ExamType;
            exam.StatusId = status.StatusId;
            exam.UpdatedAt = DateTime.UtcNow;

            // Handle Package Exam mapping
            var currentPackageExam = await _context.PackageExams
                .FirstOrDefaultAsync(pe => pe.ExamId == id);

            if (request.IsPremium && currentPackageExam == null)
            {
                var premiumPkg = await _context.Packages.FirstOrDefaultAsync(p => p.Name.Contains("Premium"));
                if (premiumPkg != null)
                {
                    _context.PackageExams.Add(new PackageExam
                    {
                        PackageExamId = Guid.NewGuid(),
                        PackageId = premiumPkg.PackageId,
                        ExamId = exam.ExamId,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            else if (!request.IsPremium && currentPackageExam != null)
            {
                _context.PackageExams.Remove(currentPackageExam);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Exams/{id} (Admin only)
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExam(Guid id)
        {
            var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
            if (roleClaim == null || roleClaim.Value != "admin")
                return Forbid();

            var exam = await _context.Exams.FindAsync(id);
            if (exam == null) return NotFound("Đề thi không tồn tại.");

            // Hard delete of exam removes package mappings, questions, and attempts cascade
            _context.Exams.Remove(exam);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
