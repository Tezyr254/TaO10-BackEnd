using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.Common;
using TaO10_BackEnd.DTOs.Exams;
using TaO10_BackEnd.Helpers;
using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Services;

public class ExamImportService : IExamImportService
{
    private const string TrialPackageName = "Gói Dùng Thử";
    private const string NotApplicable = "N/A";

    private readonly AppDbContext _context;
    private readonly ILogger<ExamImportService> _logger;

    public ExamImportService(AppDbContext context, ILogger<ExamImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ExamImportResultDto> ImportFromExcelAsync(Stream excelStream, CancellationToken cancellationToken = default)
    {
        var rows = ExamExcelParser.Parse(excelStream);
        var result = new ExamImportResultDto { PackageName = TrialPackageName };

        var examStatus = await _context.Statuses
            .FirstOrDefaultAsync(s =>
                s.EntityType == AppStatusCodes.EntityTypes.Exam &&
                s.Code == AppStatusCodes.Exams.Active, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy status ACTIVE cho Exam.");

        var questionStatus = await _context.Statuses
            .FirstOrDefaultAsync(s =>
                s.EntityType == AppStatusCodes.EntityTypes.Question &&
                s.Code == AppStatusCodes.Questions.Active, cancellationToken)
            ?? throw new InvalidOperationException("Không tìm thấy status ACTIVE cho Question.");

        var trialPackage = await _context.Packages
            .Include(p => p.PackageExams)
            .FirstOrDefaultAsync(p => p.Name == TrialPackageName, cancellationToken)
            ?? throw new InvalidOperationException($"Không tìm thấy gói '{TrialPackageName}'.");

        var linkedExamIds = trialPackage.PackageExams
            .Select(pe => pe.ExamId)
            .ToHashSet();

        var groupedExams = rows
            .GroupBy(r => r.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var examGroup in groupedExams)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var firstRow = examGroup.First();
                var existingExam = await _context.Exams
                    .FirstOrDefaultAsync(e => e.Title == examGroup.Key, cancellationToken);

                Exam exam;
                if (existingExam != null)
                {
                    exam = existingExam;
                    result.ExamsSkipped++;
                    result.SkippedExamTitles.Add(examGroup.Key);
                }
                else
                {
                    var gradableCount = examGroup.Count(r => !IsPassageRow(r));

                    exam = new Exam
                    {
                        ExamId = Guid.NewGuid(),
                        Title = firstRow.Title,
                        Description = firstRow.Description,
                        QuestionsCount = gradableCount,
                        DurationTime = firstRow.DurationTime > 0 ? firstRow.DurationTime : 60,
                        Level = Truncate(firstRow.Level, 10),
                        Year = firstRow.Year > 0 ? firstRow.Year : DateTime.UtcNow.Year,
                        ExamType = Truncate(firstRow.ExamType, 100),
                        ViewsCount = firstRow.ViewsCount,
                        AttemptsCount = firstRow.AttemptsCount,
                        StatusId = examStatus.StatusId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Exams.Add(exam);
                    result.ExamsCreated++;
                    result.CreatedExamTitles.Add(examGroup.Key);

                    foreach (var row in examGroup.OrderBy(r => r.QuestionNumber))
                    {
                        var question = MapQuestion(row, exam.ExamId, questionStatus.StatusId);
                        _context.Questions.Add(question);
                        result.QuestionsCreated++;
                    }
                }

                if (!linkedExamIds.Contains(exam.ExamId))
                {
                    _context.PackageExams.Add(new PackageExam
                    {
                        PackageExamId = Guid.NewGuid(),
                        PackageId = trialPackage.PackageId,
                        ExamId = exam.ExamId,
                        CreatedAt = DateTime.UtcNow
                    });
                    linkedExamIds.Add(exam.ExamId);
                    result.PackageLinksCreated++;
                }
            }

            trialPackage.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Imported exams from Excel. Created: {Created}, Skipped: {Skipped}, Questions: {Questions}, PackageLinks: {Links}",
                result.ExamsCreated, result.ExamsSkipped, result.QuestionsCreated, result.PackageLinksCreated);

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static Question MapQuestion(ExamExcelImportRow row, Guid examId, Guid questionStatusId)
    {
        var isPassage = IsPassageRow(row);

        return new Question
        {
            QuestionId = Guid.NewGuid(),
            ExamId = examId,
            QuestionNumber = row.QuestionNumber,
            Section = row.Section,
            QuestionText = row.QuestionText,
            OptionA = isPassage ? null : NullIfNa(row.OptionA),
            OptionB = isPassage ? null : NullIfNa(row.OptionB),
            OptionC = isPassage ? null : NullIfNa(row.OptionC),
            OptionD = isPassage ? null : NullIfNa(row.OptionD),
            CorrectAnswer = isPassage ? null : Truncate(NullIfNa(row.CorrectAnswer), 10),
            Explanation = string.IsNullOrWhiteSpace(row.Explanation) ? "Không có giải thích." : row.Explanation,
            Points = row.Points,
            StatusId = questionStatusId,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static bool IsPassageRow(ExamExcelImportRow row) =>
        string.Equals(row.CorrectAnswer, NotApplicable, StringComparison.OrdinalIgnoreCase);

    private static string? NullIfNa(string? value) =>
        string.IsNullOrWhiteSpace(value) || string.Equals(value, NotApplicable, StringComparison.OrdinalIgnoreCase)
            ? null
            : value.Trim();

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
