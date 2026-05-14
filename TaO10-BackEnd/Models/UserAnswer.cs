using System;
using System.Collections.Generic;

namespace TaO10_BackEnd.Models;

public partial class UserAnswer
{
    public Guid UserAnswerId { get; set; }

    public Guid? UserExamAttemptId { get; set; }

    public Guid? QuestionId { get; set; }

    public char? UserAnswer1 { get; set; }

    public bool? IsCorrect { get; set; }

    public DateTime? AnsweredAt { get; set; }

    public virtual Question? Question { get; set; }

    public virtual UserExamAttempt? UserExamAttempt { get; set; }
}
