using System;
using System.Collections.Generic;

namespace TaO10_BackEnd.Models;

public partial class UserExamAttempt
{
    public Guid UserExamAttemptId { get; set; }

    public Guid? UserId { get; set; }

    public Guid? ExamId { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public decimal? Score { get; set; }

    public int? CorrectAnswers { get; set; }

    public int? TotalQuestions { get; set; }

    public int? TimeSpentMinutes { get; set; }

    public Guid StatusId { get; set; }

    public virtual Exam? Exam { get; set; }

    public virtual Status Status { get; set; } = null!;

    public virtual User? User { get; set; }

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
