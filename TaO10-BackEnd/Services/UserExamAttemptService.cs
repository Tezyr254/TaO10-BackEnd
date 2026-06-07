using TaO10_BackEnd.Common;
using TaO10_BackEnd.DTOs.Exams;
using TaO10_BackEnd.Exceptions;
using TaO10_BackEnd.Mappers;
using TaO10_BackEnd.Models;
using TaO10_BackEnd.Repositories;

namespace TaO10_BackEnd.Services;

/// <summary>
/// Service for user exam attempt operations
/// </summary>
public class UserExamAttemptService : IUserExamAttemptService
{
    private readonly IUserExamAttemptRepository _attemptRepository;
    private readonly IExamRepository _examRepository;
    private readonly IQuestionRepository _questionRepository;
    private readonly IUserAnswerRepository _answerRepository;
    private readonly IStatusRepository _statusRepository;
    private readonly IExamMapper _mapper;
    private readonly ILogger<UserExamAttemptService> _logger;

    /// <summary>
    /// Initializes a new instance of the UserExamAttemptService class
    /// </summary>
    public UserExamAttemptService(
        IUserExamAttemptRepository attemptRepository,
        IExamRepository examRepository,
        IQuestionRepository questionRepository,
        IUserAnswerRepository answerRepository,
        IStatusRepository statusRepository,
        IExamMapper mapper,
        ILogger<UserExamAttemptService> logger)
    {
        _attemptRepository = attemptRepository;
        _examRepository = examRepository;
        _questionRepository = questionRepository;
        _answerRepository = answerRepository;
        _statusRepository = statusRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Starts a new exam attempt
    /// </summary>
    public async Task<UserExamAttemptDto> StartExamAsync(StartExamRequest request)
    {
        _logger.LogInformation("Starting exam. User ID: {UserId}, Exam ID: {ExamId}", request.UserId, request.ExamId);

        // Validate exam exists
        var exam = await _examRepository.GetByIdWithQuestionsAsync(request.ExamId);
        if (exam == null)
        {
            _logger.LogWarning("Exam not found with ID: {ExamId}", request.ExamId);
            throw new ResourceNotFoundException($"Exam with ID {request.ExamId} not found", "EXAM_NOT_FOUND");
        }

        // Validate exam is published
        var completedStatus = await _statusRepository
            .FindByEntityTypeAndCodeAsync("ATTEMPT", "SUBMITTED");

        _logger.LogInformation(
            "Status found: {Status}",
            completedStatus == null ? "NULL" : completedStatus.StatusId.ToString()
        );

        // Get in_progress status
        var inProgressStatus = await _statusRepository.FindByEntityTypeAndCodeAsync("ATTEMPT", "IN_PROGRESS");
        if (inProgressStatus == null)
        {
            _logger.LogError("Status 'in_progress' not found for user_exam_attempt entity");
            throw new ResourceNotFoundException("Status not found", "STATUS_NOT_FOUND");
        }

        // Create new attempt
        var attempt = _mapper.MapToUserExamAttempt(request, inProgressStatus.StatusId);

        await _attemptRepository.AddAsync(attempt);
        await _attemptRepository.SaveChangesAsync();

        // Increment exam attempts count
        await _examRepository.IncrementAttemptsCountAsync(request.ExamId);

        _logger.LogInformation("Exam started successfully. Attempt ID: {AttemptId}", attempt.UserExamAttemptId);

        // Return attempt with exam and questions
        attempt = await _attemptRepository.GetByIdWithAnswersAsync(attempt.UserExamAttemptId);
        return _mapper.MapToUserExamAttemptDto(attempt, includeQuestions: true, includeAnswers: false);
    }

    /// <summary>
    /// Gets an attempt by ID
    /// </summary>
    public async Task<UserExamAttemptDto> GetAttemptByIdAsync(Guid attemptId)
    {
        _logger.LogInformation("Getting attempt by ID: {AttemptId}", attemptId);

        var attempt = await _attemptRepository.GetByIdFullAsync(attemptId);
        if (attempt == null)
        {
            _logger.LogWarning("Attempt not found with ID: {AttemptId}", attemptId);
            throw new ResourceNotFoundException($"Attempt with ID {attemptId} not found", "ATTEMPT_NOT_FOUND");
        }

        bool isCompleted = string.Equals(
            attempt.Status?.Code,
            AppStatusCodes.Attempts.Submitted,
            StringComparison.OrdinalIgnoreCase);
        return _mapper.MapToUserExamAttemptDto(attempt, includeQuestions: true, includeAnswers: isCompleted);
    }

    /// <summary>
    /// Gets all attempts for a user
    /// </summary>
    public async Task<(List<UserExamAttemptDto> Attempts, int TotalCount)> GetUserAttemptsAsync(Guid userId, int pageNumber = 1, int pageSize = 10)
    {
        _logger.LogInformation("Getting attempts for user. User ID: {UserId}, Page: {PageNumber}, PageSize: {PageSize}", userId, pageNumber, pageSize);

        var (attempts, totalCount) = await _attemptRepository.GetUserAttemptsAsync(userId, pageNumber, pageSize);
        var attemptDtos = attempts.Select(a => _mapper.MapToUserExamAttemptDto(a, includeQuestions: false, includeAnswers: false)).ToList();

        return (attemptDtos, totalCount);
    }

    /// <summary>
    /// Submits an answer for a question
    /// </summary>
    public async Task<UserAnswerDto> SubmitAnswerAsync(Guid attemptId, SubmitAnswerRequest request)
    {
        _logger.LogInformation("Submitting answer. Attempt ID: {AttemptId}, Question ID: {QuestionId}, Answer: {Answer}", 
            attemptId, request.QuestionId, request.UserAnswer);

        // Validate attempt exists and is in progress
        var attempt = await _attemptRepository.GetByIdWithStatusAsync(attemptId);
        if (attempt == null)
        {
            _logger.LogWarning("Attempt not found with ID: {AttemptId}", attemptId);
            throw new ResourceNotFoundException($"Attempt with ID {attemptId} not found", "ATTEMPT_NOT_FOUND");
        }

        if (attempt.Status?.Code != "IN_PROGRESS")
        {
            _logger.LogWarning("Attempt is not in progress. ID: {AttemptId}, Status: {StatusCode}", attemptId, attempt.Status?.Code);
            throw new ExamAlreadyCompletedException("Attempt is not in progress", "ATTEMPT_NOT_IN_PROGRESS");
        }

        // Validate question exists and is published
        var question = await _questionRepository.GetByIdWithStatusAsync(request.QuestionId);
        if (question == null)
        {
            _logger.LogWarning("Question not found with ID: {QuestionId}", request.QuestionId);
            throw new ResourceNotFoundException($"Question with ID {request.QuestionId} not found", "QUESTION_NOT_FOUND");
        }

        if (question.Status?.EntityType != AppStatusCodes.EntityTypes.Question ||
            question.Status?.Code != AppStatusCodes.Questions.Active)
        {
            _logger.LogWarning("Question is not active. ID: {QuestionId}, Status: {StatusCode}", request.QuestionId, question.Status?.Code);
            throw new InvalidAnswerException("Question is not available", "QUESTION_NOT_ACTIVE");
        }

        // Validate question belongs to this exam
        if (question.ExamId != attempt.ExamId)
        {
            _logger.LogWarning("Question does not belong to this exam. Question ID: {QuestionId}, Exam ID: {ExamId}", request.QuestionId, attempt.ExamId);
            throw new InvalidAnswerException("Question does not belong to this exam", "QUESTION_MISMATCH");
        }

        // Validate answer format
        var normalizedAnswer = request.UserAnswer.Trim().ToUpperInvariant();
        if (normalizedAnswer.Length != 1 || !normalizedAnswer.All(c => "ABCD".Contains(c)))
        {
            _logger.LogWarning("Invalid answer format. Answer: {Answer}", request.UserAnswer);
            throw new InvalidAnswerException("Answer must be A, B, C, or D", "INVALID_ANSWER_FORMAT");
        }

        bool isCorrect = normalizedAnswer == question.CorrectAnswer?.Trim().ToUpperInvariant();

        // Update existing answer so users can change their selection before submit
        var existingAnswer = await _answerRepository.FindByAttemptAndQuestionAsync(attemptId, request.QuestionId);
        if (existingAnswer != null)
        {
            existingAnswer.UserAnswer1 = normalizedAnswer[0];
            existingAnswer.IsCorrect = isCorrect;
            existingAnswer.AnsweredAt = DateTime.UtcNow;

            await _answerRepository.UpdateAsync(existingAnswer);
            await _answerRepository.SaveChangesAsync();

            _logger.LogInformation("Answer updated successfully. Answer ID: {AnswerId}, IsCorrect: {IsCorrect}", existingAnswer.UserAnswerId, isCorrect);

            return _mapper.MapToUserAnswerDto(existingAnswer);
        }

        // Create user answer
        request.UserAnswer = normalizedAnswer;
        var userAnswer = _mapper.MapToUserAnswer(request, attemptId, request.QuestionId, isCorrect);

        await _answerRepository.AddAsync(userAnswer);
        await _answerRepository.SaveChangesAsync();

        _logger.LogInformation("Answer submitted successfully. Answer ID: {AnswerId}, IsCorrect: {IsCorrect}", userAnswer.UserAnswerId, isCorrect);

        return _mapper.MapToUserAnswerDto(userAnswer);
    }

    /// <summary>
    /// Submits the exam (completes the attempt)
    /// </summary>
    public async Task<UserExamAttemptDto> SubmitExamAsync(Guid attemptId, SubmitExamRequest request)
    {
        _logger.LogInformation("Submitting exam. Attempt ID: {AttemptId}", attemptId);

        // Validate attempt exists and is in progress
        var attempt = await _attemptRepository.GetByIdFullAsync(attemptId);
        if (attempt == null)
        {
            _logger.LogWarning("Attempt not found with ID: {AttemptId}", attemptId);
            throw new ResourceNotFoundException($"Attempt with ID {attemptId} not found", "ATTEMPT_NOT_FOUND");
        }

        if (attempt.Status?.Code != "IN_PROGRESS")
        {
            _logger.LogWarning("Attempt is not in progress. ID: {AttemptId}, Status: {StatusCode}", attemptId, attempt.Status?.Code);
            throw new ExamAlreadyCompletedException("Attempt is not in progress", "ATTEMPT_NOT_IN_PROGRESS");
        }

        // Calculate score
        var answers = attempt.UserAnswers?.ToList() ?? new List<UserAnswer>();
        int correctAnswers = answers.Count(a => a.IsCorrect ?? false);
        int totalQuestions = attempt.Exam?.QuestionsCount ?? answers.Count;

        decimal score = totalQuestions > 0
            ? (decimal)correctAnswers / totalQuestions * 100
            : 0;

        // Get completed status
        var completedStatus = await _statusRepository.FindByEntityTypeAndCodeAsync("ATTEMPT", "SUBMITTED");
        if (completedStatus == null)
        {
            _logger.LogError("Status 'completed' not found for user_exam_attempt entity");
            throw new ResourceNotFoundException("Status not found", "STATUS_NOT_FOUND");
        }

        // Update attempt
        attempt.CompletedAt = DateTime.UtcNow;
        attempt.Score = score;
        attempt.CorrectAnswers = correctAnswers;
        attempt.TotalQuestions = totalQuestions;
        attempt.TimeSpentMinutes = attempt.StartedAt.HasValue
            ? (int)(attempt.CompletedAt.Value - attempt.StartedAt.Value).TotalMinutes
            : 0;
        attempt.StatusId = completedStatus.StatusId;
        attempt.Status = completedStatus;

        await _attemptRepository.UpdateAsync(attempt);
        await _attemptRepository.SaveChangesAsync();

        _logger.LogInformation("Exam submitted successfully. Attempt ID: {AttemptId}, Score: {Score}, CorrectAnswers: {CorrectAnswers}/{TotalQuestions}", 
            attemptId, score, correctAnswers, totalQuestions);

        return _mapper.MapToUserExamAttemptDto(attempt, includeQuestions: true, includeAnswers: true);
    }
}
