namespace TaO10_BackEnd.DTOs.Exams;

/// <summary>
/// DTO for Question
/// </summary>
public class QuestionDto
{
    /// <summary>
    /// Unique identifier for the question
    /// </summary>
    public Guid QuestionId { get; set; }

    /// <summary>
    /// Question number within the exam
    /// </summary>
    public int QuestionNumber { get; set; }

    /// <summary>
    /// Section of the exam (e.g., "Listening", "Reading")
    /// </summary>
    public string? Section { get; set; }

    /// <summary>
    /// The question text
    /// </summary>
    public string QuestionText { get; set; } = null!;

    /// <summary>
    /// Option A
    /// </summary>
    public string? OptionA { get; set; }

    /// <summary>
    /// Option B
    /// </summary>
    public string? OptionB { get; set; }

    /// <summary>
    /// Option C
    /// </summary>
    public string? OptionC { get; set; }

    /// <summary>
    /// Option D
    /// </summary>
    public string? OptionD { get; set; }

    /// <summary>
    /// Correct answer (A/B/C/D) - only included when appropriate
    /// </summary>
    public string? CorrectAnswer { get; set; }

    /// <summary>
    /// Explanation for the correct answer
    /// </summary>
    public string? Explanation { get; set; }

    /// <summary>
    /// Points assigned to this question
    /// </summary>
    public decimal? Points { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime? CreatedAt { get; set; }
}
