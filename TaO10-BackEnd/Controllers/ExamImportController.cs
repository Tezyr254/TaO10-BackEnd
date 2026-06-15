using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaO10_BackEnd.Common;
using TaO10_BackEnd.DTOs.Exams;
using TaO10_BackEnd.Services;

namespace TaO10_BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExamImportController : ControllerBase
{
    private static readonly string[] AllowedExtensions = [".xlsx"];

    private readonly IExamImportService _importService;
    private readonly ILogger<ExamImportController> _logger;

    public ExamImportController(IExamImportService importService, ILogger<ExamImportController> logger)
    {
        _importService = importService;
        _logger = logger;
    }

    /// <summary>
    /// Import đề thi và câu hỏi từ file Excel, gắn vào gói "Gói Dùng Thử".
    /// </summary>
    [Authorize]
    [HttpPost("excel")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> ImportFromExcel(IFormFile file, CancellationToken cancellationToken)
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role) ?? User.FindFirst("role");
        if (roleClaim == null || !string.Equals(roleClaim.Value, "admin", StringComparison.OrdinalIgnoreCase))
            return Forbid();

        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<ExamImportResultDto>.ErrorResponse(
                "Vui lòng chọn file Excel (.xlsx).",
                "FILE_REQUIRED",
                400));
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(ApiResponse<ExamImportResultDto>.ErrorResponse(
                "Chỉ hỗ trợ file .xlsx.",
                "INVALID_FILE_TYPE",
                400));
        }

        try
        {
            await using var stream = file.OpenReadStream();
            var result = await _importService.ImportFromExcelAsync(stream, cancellationToken);

            return Ok(ApiResponse<ExamImportResultDto>.SuccessResponse(
                result,
                $"Import thành công: {result.ExamsCreated} đề mới, {result.QuestionsCreated} câu hỏi, {result.PackageLinksCreated} liên kết gói.",
                200));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Import Excel validation failed");
            return BadRequest(ApiResponse<ExamImportResultDto>.ErrorResponse(ex.Message, "IMPORT_VALIDATION_ERROR", 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import Excel failed");
            return StatusCode(500, ApiResponse<ExamImportResultDto>.ErrorResponse(
                "Có lỗi xảy ra khi import file Excel.",
                "IMPORT_FAILED",
                500));
        }
    }
}
