using TaO10_BackEnd.DTOs.Exams;

namespace TaO10_BackEnd.Services;

public interface IExamImportService
{
    Task<ExamImportResultDto> ImportFromExcelAsync(Stream excelStream, CancellationToken cancellationToken = default);
}
