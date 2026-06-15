namespace TaO10_BackEnd.DTOs.Exams;

/// <summary>
/// Request DTO for submitting an exam
/// </summary>
public class SubmitExamRequest
{
    /// <summary>
    /// Additional notes or feedback (optional)
    /// </summary>
    public string? Notes { get; set; }
}
