using System.ComponentModel.DataAnnotations;

namespace TaO10_BackEnd.DTOs.Exams;

/// <summary>
/// Request DTO for submitting a single answer
/// </summary>
public class SubmitAnswerRequest
{
    /// <summary>
    /// ID of the question being answered
    /// </summary>
    [Required(ErrorMessage = "QuestionId is required")]
    public Guid QuestionId { get; set; }

    /// <summary>
    /// The answer (A/B/C/D)
    /// </summary>
    [Required(ErrorMessage = "UserAnswer is required")]
    [RegularExpression("^[A-D]$", ErrorMessage = "UserAnswer must be A, B, C, or D")]
    public string UserAnswer { get; set; } = null!;
}
