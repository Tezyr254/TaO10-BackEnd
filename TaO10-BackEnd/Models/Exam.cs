using System;
using System.Collections.Generic;

namespace TaO10_BackEnd.Models;

public partial class Exam
{
    public Guid ExamId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? QuestionsCount { get; set; }

    public int DurationTime { get; set; }

    public string? Level { get; set; }

    public int? Year { get; set; }

    public string? ExamType { get; set; }

    public int? ViewsCount { get; set; }

    public int? AttemptsCount { get; set; }

    public Guid StatusId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<PackageExam> PackageExams { get; set; } = new List<PackageExam>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual Status Status { get; set; } = null!;

    public virtual ICollection<UserExamAttempt> UserExamAttempts { get; set; } = new List<UserExamAttempt>();
}
