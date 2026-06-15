using TaO10_BackEnd.Common;
using TaO10_BackEnd.DTOs.Exams;
using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Mappers;

/// <summary>
/// Interface for exam-related mapping operations
/// </summary>
public interface IExamMapper
{
    /// <summary>
    /// Maps an Exam entity to ExamDto
    /// </summary>
    ExamDto MapToExamDto(Exam exam);

    /// <summary>
    /// Maps a Question entity to QuestionDto
    /// </summary>
    QuestionDto MapToQuestionDto(Question question, bool includeCorrectAnswer = true);

    /// <summary>
    /// Maps a UserExamAttempt entity to UserExamAttemptDto
    /// </summary>
    UserExamAttemptDto MapToUserExamAttemptDto(UserExamAttempt attempt, bool includeQuestions = false, bool includeAnswers = false);

    /// <summary>
    /// Maps a UserAnswer entity to UserAnswerDto
    /// </summary>
    UserAnswerDto MapToUserAnswerDto(UserAnswer answer);

    /// <summary>
    /// Maps a list of questions to QuestionDto list
    /// </summary>
    List<QuestionDto> MapToQuestionDtoList(IEnumerable<Question> questions, bool includeCorrectAnswer = true);

    /// <summary>
    /// Maps a list of answers to UserAnswerDto list
    /// </summary>
    List<UserAnswerDto> MapToUserAnswerDtoList(IEnumerable<UserAnswer> answers);

    /// <summary>
    /// Maps StartExamRequest to UserExamAttempt entity
    /// </summary>
    UserExamAttempt MapToUserExamAttempt(StartExamRequest request, Guid statusIdInProgress);

    /// <summary>
    /// Maps SubmitAnswerRequest to UserAnswer entity
    /// </summary>
    UserAnswer MapToUserAnswer(SubmitAnswerRequest request, Guid attemptId, Guid questionId, bool isCorrect);
}

/// <summary>
/// Mapper for exam-related entities
/// </summary>
public class ExamMapper : IExamMapper
{
    /// <summary>
    /// Maps an Exam entity to ExamDto
    /// </summary>
    public ExamDto MapToExamDto(Exam exam)
    {
        return new ExamDto
        {
            ExamId = exam.ExamId,
            Title = exam.Title,
            Description = exam.Description,
            QuestionsCount = exam.QuestionsCount,
            DurationTime = exam.DurationTime,
            Level = exam.Level,
            Year = exam.Year,
            ExamType = exam.ExamType,
            ViewsCount = exam.ViewsCount,
            AttemptsCount = exam.AttemptsCount,
            StatusCode = exam.Status?.Code,
            CreatedAt = exam.CreatedAt,
            UpdatedAt = exam.UpdatedAt
        };
    }

    /// <summary>
    /// Maps a Question entity to QuestionDto
    /// </summary>
    public QuestionDto MapToQuestionDto(Question question, bool includeCorrectAnswer = true)
    {
        return new QuestionDto
        {
            QuestionId = question.QuestionId,
            QuestionNumber = question.QuestionNumber,
            Section = question.Section,
            QuestionText = question.QuestionText,
            OptionA = question.OptionA,
            OptionB = question.OptionB,
            OptionC = question.OptionC,
            OptionD = question.OptionD,
            CorrectAnswer = includeCorrectAnswer ? question.CorrectAnswer : null,
            Explanation = includeCorrectAnswer ? question.Explanation : null,
            Points = question.Points,
            CreatedAt = question.CreatedAt
        };
    }

    /// <summary>
    /// Maps a UserExamAttempt entity to UserExamAttemptDto
    /// </summary>
    public UserExamAttemptDto MapToUserExamAttemptDto(UserExamAttempt attempt, bool includeQuestions = false, bool includeAnswers = false)
    {
        var dto = new UserExamAttemptDto
        {
            UserExamAttemptId = attempt.UserExamAttemptId,
            UserId = attempt.UserId,
            ExamId = attempt.ExamId,
            StartedAt = attempt.StartedAt,
            CompletedAt = attempt.CompletedAt,
            Score = attempt.Score,
            CorrectAnswers = attempt.CorrectAnswers,
            TotalQuestions = attempt.TotalQuestions,
            TimeSpentMinutes = attempt.TimeSpentMinutes,
            StatusCode = attempt.Status?.Code,
            Exam = attempt.Exam != null ? MapToExamDto(attempt.Exam) : null
        };

        // Calculate percentage if score exists
        if (attempt.TotalQuestions.HasValue && attempt.TotalQuestions > 0 && attempt.CorrectAnswers.HasValue)
        {
            dto.Percentage = (decimal)attempt.CorrectAnswers.Value / attempt.TotalQuestions.Value * 100;
        }

        // Include questions if requested and not showing answers yet (avoid showing correct answers during exam)
        if (includeQuestions && attempt.Exam?.Questions != null)
        {
            bool isCompleted = IsAttemptCompleted(attempt);
            dto.Questions = MapToQuestionDtoList(attempt.Exam.Questions, includeCorrectAnswer: isCompleted);
        }

        // Include answers if requested
        if (includeAnswers && attempt.UserAnswers != null)
        {
            bool isCompleted = IsAttemptCompleted(attempt);
            dto.UserAnswers = attempt.UserAnswers.Select(ua => new UserAnswerDto
            {
                UserAnswerId = ua.UserAnswerId,
                UserExamAttemptId = ua.UserExamAttemptId,
                QuestionId = ua.QuestionId,
                UserAnswer = ua.UserAnswer1,
                IsCorrect = isCompleted ? ua.IsCorrect : null,
                AnsweredAt = ua.AnsweredAt
            }).ToList();
        }

        return dto;
    }

    /// <summary>
    /// Maps a UserAnswer entity to UserAnswerDto
    /// </summary>
    public UserAnswerDto MapToUserAnswerDto(UserAnswer answer)
    {
        return new UserAnswerDto
        {
            UserAnswerId = answer.UserAnswerId,
            UserExamAttemptId = answer.UserExamAttemptId,
            QuestionId = answer.QuestionId,
            UserAnswer = answer.UserAnswer1,
            IsCorrect = answer.IsCorrect,
            AnsweredAt = answer.AnsweredAt
        };
    }

    /// <summary>
    /// Maps a list of questions to QuestionDto list
    /// </summary>
    public List<QuestionDto> MapToQuestionDtoList(IEnumerable<Question> questions, bool includeCorrectAnswer = true)
    {
        return questions.Select(q => MapToQuestionDto(q, includeCorrectAnswer)).ToList();
    }

    /// <summary>
    /// Maps a list of answers to UserAnswerDto list
    /// </summary>
    public List<UserAnswerDto> MapToUserAnswerDtoList(IEnumerable<UserAnswer> answers)
    {
        return answers.Select(MapToUserAnswerDto).ToList();
    }

    /// <summary>
    /// Maps StartExamRequest to UserExamAttempt entity
    /// </summary>
    public UserExamAttempt MapToUserExamAttempt(StartExamRequest request, Guid statusIdInProgress)
    {
        return new UserExamAttempt
        {
            UserExamAttemptId = Guid.NewGuid(),
            UserId = request.UserId,
            ExamId = request.ExamId,
            StartedAt = DateTime.UtcNow,
            StatusId = statusIdInProgress,
            CorrectAnswers = 0,
            TotalQuestions = 0,
            TimeSpentMinutes = 0
        };
    }

    private static bool IsAttemptCompleted(UserExamAttempt attempt)
    {
        var code = attempt.Status?.Code;
        return string.Equals(code, AppStatusCodes.Attempts.Submitted, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Maps SubmitAnswerRequest to UserAnswer entity
    /// </summary>
    public UserAnswer MapToUserAnswer(SubmitAnswerRequest request, Guid attemptId, Guid questionId, bool isCorrect)
    {
        return new UserAnswer
        {
            UserAnswerId = Guid.NewGuid(),
            UserExamAttemptId = attemptId,
            QuestionId = questionId,
            UserAnswer1 = char.Parse(request.UserAnswer.ToUpper()),
            IsCorrect = isCorrect,
            AnsweredAt = DateTime.UtcNow
        };
    }
}
