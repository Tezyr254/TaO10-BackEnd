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

    /// <summary>
    /// Optional client-side anti-cheat report collected during the attempt.
    /// </summary>
    public ExamSecurityReportDto? SecurityReport { get; set; }
}

public class ExamSecurityReportDto
{
    public string? Reason { get; set; }

    public int AltTabCount { get; set; }

    public int Threshold { get; set; }

    public int TotalAwaySeconds { get; set; }

    public bool AutoSubmitted { get; set; }

    public string? StudentEmail { get; set; }

    public string? StudentUserId { get; set; }

    public string? ClientTimeZone { get; set; }

    public string? UserAgent { get; set; }

    public List<ExamSecurityEventDto> Events { get; set; } = new();
}

public class ExamSecurityEventDto
{
    public int Index { get; set; }

    public DateTime? LeftAt { get; set; }

    public DateTime? ReturnedAt { get; set; }

    public int DurationSeconds { get; set; }
}