using System.ComponentModel.DataAnnotations;

namespace TaO10_BackEnd.DTOs.Exams;

/// <summary>
/// Request DTO for starting an exam
/// </summary>
public class StartExamRequest
{
    /// <summary>
    /// The ID of the exam to start
    /// </summary>
    [Required(ErrorMessage = "ExamId is required")]
    public Guid ExamId { get; set; }

    /// <summary>
    /// The ID of the user starting the exam. Optional for guest attempts.
    /// </summary>
    public Guid? UserId { get; set; }
}
