namespace TaO10_BackEnd.DTOs.Exams;

/// <summary>
/// DTO for Exam - Read-only (without questions)
/// </summary>
public class ExamDto
{
    /// <summary>
    /// Unique identifier for the exam
    /// </summary>
    public Guid ExamId { get; set; }

    /// <summary>
    /// Title of the exam
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// Description of the exam
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Total number of questions in the exam
    /// </summary>
    public int? QuestionsCount { get; set; }

    /// <summary>
    /// Duration in minutes
    /// </summary>
    public int DurationTime { get; set; }

    /// <summary>
    /// Difficulty level (e.g., "easy", "medium", "hard")
    /// </summary>
    public string? Level { get; set; }

    /// <summary>
    /// Year of the exam
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// Type of exam (e.g., "practice", "official")
    /// </summary>
    public string? ExamType { get; set; }

    /// <summary>
    /// Number of times the exam has been viewed
    /// </summary>
    public int? ViewsCount { get; set; }

    /// <summary>
    /// Number of times the exam has been attempted
    /// </summary>
    public int? AttemptsCount { get; set; }

    /// <summary>
    /// Status code (e.g., "active", "inactive")
    /// </summary>
    public string? StatusCode { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    public bool IsPremium { get;  set; }
    public int? ProgressPercentage { get;  set; }
}
