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

    /// <summary>
    /// Initializes a new instance of the ExamService class
    /// </summary>
    public ExamService(
        IExamRepository examRepository,
        IQuestionRepository questionRepository,
        IExamMapper mapper,
        ILogger<ExamService> logger)
    {
        _examRepository = examRepository;
        _questionRepository = questionRepository;
        _mapper = mapper;
        _logger = logger;
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
    public async Task<ExamResponseDto> GetExamWithQuestionsAsync(Guid examId)
    {
        _logger.LogInformation("Getting exam with questions. Exam ID: {ExamId}", examId);

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
