using Microsoft.AspNetCore.Mvc;
using TaO10_BackEnd.Common;
using TaO10_BackEnd.DTOs.Exams;
using TaO10_BackEnd.Exceptions;
using TaO10_BackEnd.Services;

namespace TaO10_BackEnd.Controllers;

/// <summary>
/// Controller for user exam attempt operations
/// </summary>
[ApiController]
[Route("api/user/exam-attempts")]
public class UserExamAttemptsController : ControllerBase
{
    private readonly IUserExamAttemptService _attemptService;
    private readonly ILogger<UserExamAttemptsController> _logger;

    /// <summary>
    /// Initializes a new instance of the UserExamAttemptsController class
    /// </summary>
    public UserExamAttemptsController(IUserExamAttemptService attemptService, ILogger<UserExamAttemptsController> logger)
    {
        _attemptService = attemptService;
        _logger = logger;
    }

    /// <summary>
    /// Starts a new exam attempt
    /// </summary>
    /// <param name="request">Start exam request</param>
    /// <returns>Exam attempt details with questions</returns>
    [HttpPost("start")]
    public async Task<IActionResult> StartExam([FromBody] StartExamRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<UserExamAttemptDto>.ErrorResponse(
                    string.Join(", ", errors),
                    "VALIDATION_ERROR",
                    400));
            }

            _logger.LogInformation("StartExam called with User ID: {UserId}, Exam ID: {ExamId}", request.UserId, request.ExamId);

            var attempt = await _attemptService.StartExamAsync(request);
            return CreatedAtAction(nameof(GetAttemptById), new { id = attempt.UserExamAttemptId }, 
                ApiResponse<UserExamAttemptDto>.SuccessResponse(attempt, "Exam started successfully", 201));
        }
        catch (ResourceNotFoundException ex)
        {
            _logger.LogWarning("Resource not found: {Message}", ex.Message);
            return NotFound(ApiResponse<UserExamAttemptDto>.ErrorResponse(ex.Message, ex.ErrorCode, 404));
        }
        catch (ExamAccessDeniedException ex)
        {
            _logger.LogWarning("Access denied: {Message}", ex.Message);
            return BadRequest(ApiResponse<UserExamAttemptDto>.ErrorResponse(ex.Message, ex.ErrorCode, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in StartExam");
            return StatusCode(500, ApiResponse<UserExamAttemptDto>.ErrorResponse("An error occurred", "INTERNAL_ERROR", 500));
        }
    }

    /// <summary>
    /// Gets an attempt by ID
    /// </summary>
    /// <param name="id">Attempt ID</param>
    /// <returns>Attempt details</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAttemptById(Guid id)
    {
        try
        {
            _logger.LogInformation("GetAttemptById called with ID: {AttemptId}", id);

            var attempt = await _attemptService.GetAttemptByIdAsync(id);
            return Ok(ApiResponse<UserExamAttemptDto>.SuccessResponse(attempt, "Attempt retrieved successfully", 200));
        }
        catch (ResourceNotFoundException ex)
        {
            _logger.LogWarning("Resource not found: {Message}", ex.Message);
            return NotFound(ApiResponse<UserExamAttemptDto>.ErrorResponse(ex.Message, ex.ErrorCode, 404));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAttemptById");
            return StatusCode(500, ApiResponse<UserExamAttemptDto>.ErrorResponse("An error occurred", "INTERNAL_ERROR", 500));
        }
    }

    /// <summary>
    /// Gets all attempts for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>List of attempts</returns>
    [HttpGet]
    public async Task<IActionResult> GetUserAttempts([FromQuery] Guid userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (userId == Guid.Empty)
            {
                return BadRequest(ApiResponse<List<UserExamAttemptDto>>.ErrorResponse(
                    "UserId is required",
                    "VALIDATION_ERROR",
                    400));
            }

            if (pageNumber < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse<List<UserExamAttemptDto>>.ErrorResponse(
                    "Page number and page size must be greater than 0",
                    "INVALID_PAGINATION",
                    400));
            }

            _logger.LogInformation("GetUserAttempts called with User ID: {UserId}, Page: {PageNumber}, PageSize: {PageSize}", userId, pageNumber, pageSize);

            var (attempts, totalCount) = await _attemptService.GetUserAttemptsAsync(userId, pageNumber, pageSize);

            var response = new
            {
                attempts,
                totalCount,
                pageNumber,
                pageSize,
                totalPages = (int)Math.Ceiling((decimal)totalCount / pageSize)
            };

            return Ok(ApiResponse<object>.SuccessResponse(response, "Attempts retrieved successfully", 200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetUserAttempts");
            return StatusCode(500, ApiResponse<List<UserExamAttemptDto>>.ErrorResponse("An error occurred", "INTERNAL_ERROR", 500));
        }
    }

    /// <summary>
    /// Submits an answer for a question
    /// </summary>
    /// <param name="id">Attempt ID</param>
    /// <param name="request">Answer submission request</param>
    /// <returns>Answer details</returns>
    [HttpPost("{id}/answer")]
    public async Task<IActionResult> SubmitAnswer(Guid id, [FromBody] SubmitAnswerRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<UserAnswerDto>.ErrorResponse(
                    string.Join(", ", errors),
                    "VALIDATION_ERROR",
                    400));
            }

            _logger.LogInformation("SubmitAnswer called with Attempt ID: {AttemptId}, Question ID: {QuestionId}", id, request.QuestionId);

            var answer = await _attemptService.SubmitAnswerAsync(id, request);
            return Ok(ApiResponse<UserAnswerDto>.SuccessResponse(answer, "Answer submitted successfully", 200));
        }
        catch (ResourceNotFoundException ex)
        {
            _logger.LogWarning("Resource not found: {Message}", ex.Message);
            return NotFound(ApiResponse<UserAnswerDto>.ErrorResponse(ex.Message, ex.ErrorCode, 404));
        }
        catch (ExamAlreadyCompletedException ex)
        {
            _logger.LogWarning("Exam already completed: {Message}", ex.Message);
            return BadRequest(ApiResponse<UserAnswerDto>.ErrorResponse(ex.Message, ex.ErrorCode, 400));
        }
        catch (InvalidAnswerException ex)
        {
            _logger.LogWarning("Invalid answer: {Message}", ex.Message);
            return BadRequest(ApiResponse<UserAnswerDto>.ErrorResponse(ex.Message, ex.ErrorCode, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SubmitAnswer");
            return StatusCode(500, ApiResponse<UserAnswerDto>.ErrorResponse("An error occurred", "INTERNAL_ERROR", 500));
        }
    }

    /// <summary>
    /// Submits the exam (completes the attempt)
    /// </summary>
    /// <param name="id">Attempt ID</param>
    /// <param name="request">Submit exam request</param>
    /// <returns>Completed attempt with score</returns>
    [HttpPost("{id}/submit")]
    public async Task<IActionResult> SubmitExam(Guid id, [FromBody] SubmitExamRequest? request = null)
    {
        try
        {
            _logger.LogInformation("SubmitExam called with Attempt ID: {AttemptId}", id);

            request ??= new SubmitExamRequest();

            var attempt = await _attemptService.SubmitExamAsync(id, request);
            return Ok(ApiResponse<UserExamAttemptDto>.SuccessResponse(attempt, "Exam submitted successfully", 200));
        }
        catch (ResourceNotFoundException ex)
        {
            _logger.LogWarning("Resource not found: {Message}", ex.Message);
            return NotFound(ApiResponse<UserExamAttemptDto>.ErrorResponse(ex.Message, ex.ErrorCode, 404));
        }
        catch (ExamAlreadyCompletedException ex)
        {
            _logger.LogWarning("Exam already completed: {Message}", ex.Message);
            return BadRequest(ApiResponse<UserExamAttemptDto>.ErrorResponse(ex.Message, ex.ErrorCode, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SubmitExam");
            return StatusCode(500, ApiResponse<UserExamAttemptDto>.ErrorResponse("An error occurred", "INTERNAL_ERROR", 500));
        }
    }
}
