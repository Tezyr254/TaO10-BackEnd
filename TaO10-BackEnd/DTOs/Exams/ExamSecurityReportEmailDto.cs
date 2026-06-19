namespace TaO10_BackEnd.DTOs.Exams;

public class ExamSecurityReportEmailDto
{
    public Guid AttemptId { get; set; }

    public Guid? UserId { get; set; }

    public string? StudentEmail { get; set; }

    public string? StudentName { get; set; }

    public Guid? ExamId { get; set; }

    public string? ExamTitle { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public decimal? Score { get; set; }

    public int? CorrectAnswers { get; set; }

    public int? TotalQuestions { get; set; }

    public string? Reason { get; set; }

    public int AltTabCount { get; set; }

    public int Threshold { get; set; }

    public int TotalAwaySeconds { get; set; }

    public bool AutoSubmitted { get; set; }

    public string? ClientTimeZone { get; set; }

    public string? UserAgent { get; set; }

    public List<ExamSecurityEventDto> Events { get; set; } = new();
}