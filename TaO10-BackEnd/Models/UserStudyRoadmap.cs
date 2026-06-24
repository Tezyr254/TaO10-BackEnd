namespace TaO10_BackEnd.Models;

public partial class UserStudyRoadmap
{
    public Guid UserStudyRoadmapId { get; set; }

    public Guid UserId { get; set; }

    public Guid UserExamAttemptId { get; set; }

    public string Summary { get; set; } = null!;

    public string Strengths { get; set; } = "[]";

    public string Weaknesses { get; set; } = "[]";

    public string Weeks { get; set; } = "[]";

    public string DailyTime { get; set; } = null!;

    public string NextAction { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual UserExamAttempt UserExamAttempt { get; set; } = null!;
}
