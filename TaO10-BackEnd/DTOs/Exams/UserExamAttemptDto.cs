namespace TaO10_BackEnd.DTOs.Exams;

/// <summary>
/// DTO for User Exam Attempt
/// </summary>
public class UserExamAttemptDto
{
    /// <summary>
    /// Unique identifier for the attempt
    /// </summary>
    public Guid UserExamAttemptId { get; set; }

    /// <summary>
    /// User ID
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Exam ID
    /// </summary>
    public Guid? ExamId { get; set; }

    /// <summary>
    /// Exam information
    /// </summary>
    public ExamDto? Exam { get; set; }

    /// <summary>
    /// Questions (included when retrieving attempt with questions)
    /// </summary>
    public List<QuestionDto>? Questions { get; set; }

    /// <summary>
    /// User's answers
    /// </summary>
    public List<UserAnswerDto>? UserAnswers { get; set; }

    /// <summary>
    /// Timestamp when the attempt started
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Timestamp when the attempt was completed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Score (0-100)
    /// </summary>
    public decimal? Score { get; set; }

    /// <summary>
    /// Percentage score (0-100)
    /// </summary>
    public decimal? Percentage { get; set; }

    /// <summary>
    /// Number of correct answers
    /// </summary>
    public int? CorrectAnswers { get; set; }

    /// <summary>
    /// Total number of questions
    /// </summary>
    public int? TotalQuestions { get; set; }

    /// <summary>
    /// Time spent in minutes
    /// </summary>
    public int? TimeSpentMinutes { get; set; }

    /// <summary>
    /// Status code (e.g., "in_progress", "completed")
    /// </summary>
    public string? StatusCode { get; set; }
}
