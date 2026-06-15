namespace TaO10_BackEnd.DTOs.Exams;

public class ExamImportResultDto
{
    public int ExamsCreated { get; set; }
    public int ExamsSkipped { get; set; }
    public int QuestionsCreated { get; set; }
    public int PackageLinksCreated { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public List<string> CreatedExamTitles { get; set; } = new();
    public List<string> SkippedExamTitles { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
