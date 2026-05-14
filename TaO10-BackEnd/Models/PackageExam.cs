using System;
using System.Collections.Generic;

namespace TaO10_BackEnd.Models;

public partial class PackageExam
{
    public Guid PackageExamId { get; set; }

    public Guid PackageId { get; set; }

    public Guid ExamId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Exam Exam { get; set; } = null!;

    public virtual Package Package { get; set; } = null!;
}
