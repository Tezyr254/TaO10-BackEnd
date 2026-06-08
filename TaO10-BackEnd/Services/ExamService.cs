using TaO10_BackEnd.Common;
using TaO10_BackEnd.DTOs.Exams;
using TaO10_BackEnd.Exceptions;
using TaO10_BackEnd.Mappers;
using TaO10_BackEnd.Repositories;

namespace TaO10_BackEnd.Services;

/// <summary>
/// Service for exam-related operations
/// </summary>
public class ExamService : IExamService
{
    private readonly IExamRepository _examRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IExamMapper _mapper;
    private readonly ILogger<ExamService> _logger;
    private readonly TaO10_BackEnd.Models.AppDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the ExamService class
    /// </summary>
    public ExamService(
        IExamRepository examRepository,
        IQuestionRepository questionRepository,
        IExamMapper mapper,
        ILogger<ExamService> logger,
        TaO10_BackEnd.Models.AppDbContext dbContext)
    {
        _examRepository = examRepository;
        _questionRepository = questionRepository;
        _mapper = mapper;
        _logger = logger;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets an exam by ID without questions
    /// </summary>
    public async Task<ExamDto> GetExamByIdAsync(Guid examId)
    {
        _logger.LogInformation("Getting exam with ID: {ExamId}", examId);

        var exam = await _examRepository.GetByIdWithStatusAsync(examId);

        if (exam == null)
        {
            _logger.LogWarning("Exam not found with ID: {ExamId}", examId);
            throw new ResourceNotFoundException($"Exam with ID {examId} not found", "EXAM_NOT_FOUND");
        }

        // Check if exam is published
        if (exam.Status?.EntityType != AppStatusCodes.EntityTypes.Exam ||
            exam.Status?.Code != AppStatusCodes.Exams.Published)
        {
            _logger.LogWarning("Exam is not published. ID: {ExamId}, Status: {StatusCode}", examId, exam.Status?.Code);
            throw new ExamAccessDeniedException("Exam is not available", "EXAM_NOT_ACTIVE");
        }

        // Increment views count
        await _examRepository.IncrementViewsCountAsync(examId);

        return _mapper.MapToExamDto(exam);
    }

    /// <summary>
    /// Gets all active exams with pagination
    /// </summary>
    public async Task<(List<ExamDto> Exams, int TotalCount)> GetAllExamsAsync(int pageNumber = 1, int pageSize = 10)
    {
        _logger.LogInformation("Getting all active exams. Page: {PageNumber}, PageSize: {PageSize}", pageNumber, pageSize);

        var (exams, totalCount) = await _examRepository.GetActiveExamsAsync(pageNumber, pageSize);

        var examDtos = exams.Select(e => _mapper.MapToExamDto(e)).ToList();

        return (examDtos, totalCount);
    }

    /// <summary>
    /// Gets an exam with all questions (without correct answers)
    /// </summary>
    public async Task<ExamResponseDto> GetExamWithQuestionsAsync(Guid examId, Guid? userId = null)
    {
        _logger.LogInformation("Getting exam with questions. Exam ID: {ExamId}, User ID: {UserId}", examId, userId);

        var exam = await _examRepository.GetByIdWithQuestionsAsync(examId);

        if (exam == null)
        {
            _logger.LogWarning("Exam not found with ID: {ExamId}", examId);
            throw new ResourceNotFoundException($"Exam with ID {examId} not found", "EXAM_NOT_FOUND");
        }

        // Check if exam is published
        if (exam.Status?.EntityType != AppStatusCodes.EntityTypes.Exam ||
            exam.Status?.Code != AppStatusCodes.Exams.Published)
        {
            _logger.LogWarning("Exam is not published. ID: {ExamId}, Status: {StatusCode}", examId, exam.Status?.Code);
            throw new ExamAccessDeniedException("Exam is not available", "EXAM_NOT_ACTIVE");
        }

        if (userId == null || userId == Guid.Empty)
        {
            _logger.LogWarning("Access denied: User is not authenticated. ID: {ExamId}", examId);
            throw new ExamAccessDeniedException("Bạn cần đăng nhập và mua gói học tập để xem đề thi này", "EXAM_ACCESS_DENIED");
        }

        // Check if user has an active UserPackage that contains this Exam
        if (_dbContext != null)
        {
            var activeUserPkgStatus = _dbContext.Statuses
                .FirstOrDefault(s => s.EntityType == "UserPackage" && s.Code == "ACTIVE");

            if (activeUserPkgStatus == null)
            {
                throw new Exception("Status 'ACTIVE' for UserPackage not configured.");
            }

            var hasAccess = _dbContext.UserPackages
                .Any(up => up.UserId == userId.Value 
                    && up.StatusId == activeUserPkgStatus.StatusId 
                    && (up.EndDate == null || up.EndDate >= DateTime.UtcNow)
                    && up.Package != null 
                    && up.Package.PackageExams.Any(pe => pe.ExamId == examId));

            if (!hasAccess)
            {
                _logger.LogWarning("Access denied: User {UserId} does not have an active package for Exam {ExamId}", userId, examId);
                throw new ExamAccessDeniedException("Bạn chưa mua gói chứa đề thi này hoặc gói đã hết hạn.", "EXAM_ACCESS_DENIED");
            }
        }

        // Increment views count
        await _examRepository.IncrementViewsCountAsync(examId);

        var examDto = _mapper.MapToExamDto(exam);
        var questionsDto = exam.Questions != null
            ? _mapper.MapToQuestionDtoList(exam.Questions, includeCorrectAnswer: false)
            : new List<QuestionDto>();

        return new ExamResponseDto
        {
            Exam = examDto,
            Questions = questionsDto
        };
    }
}
