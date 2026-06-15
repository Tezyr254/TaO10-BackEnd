namespace TaO10_BackEnd.DTOs.Exams;

/// <summary>
/// DTO for User Answer
/// </summary>
public class UserAnswerDto
{
    /// <summary>
    /// Unique identifier for the user answer
    /// </summary>
    public Guid UserAnswerId { get; set; }

    /// <summary>
    /// ID of the user exam attempt
    /// </summary>
    public Guid? UserExamAttemptId { get; set; }

    /// <summary>
    /// ID of the question
    /// </summary>
    public Guid? QuestionId { get; set; }

    /// <summary>
    /// The answer provided by the user (A/B/C/D)
    /// </summary>
    public char? UserAnswer { get; set; }

    /// <summary>
    /// Whether the answer is correct
    /// </summary>
    public bool? IsCorrect { get; set; }

    /// <summary>
    /// Timestamp when the answer was submitted
    /// </summary>
    public DateTime? AnsweredAt { get; set; }
}
