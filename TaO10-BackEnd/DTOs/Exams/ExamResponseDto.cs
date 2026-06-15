namespace TaO10_BackEnd.DTOs.Exams;

/// <summary>
/// DTO for Exam with Questions
/// </summary>
public class ExamResponseDto
{
    /// <summary>
    /// Exam information
    /// </summary>
    public ExamDto Exam { get; set; } = null!;

    /// <summary>
    /// List of questions (without correct answers for in-progress attempts)
    /// </summary>
    public List<QuestionDto> Questions { get; set; } = new();

    /// <summary>
    /// Start time of the attempt
    /// </summary>
    public DateTime? StartedAt { get; set; }

    /// <summary>
    /// Time remaining in minutes
    /// </summary>
    public int? TimeRemainingMinutes { get; set; }
}
