using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TaO10_BackEnd.Common;
using TaO10_BackEnd.DTOs.AiRoadmaps;
using TaO10_BackEnd.Models;

namespace TaO10_BackEnd.Services;

public class AiRoadmapService : IAiRoadmapService
{
    private static readonly object FallbackRoadmapLock = new();
    private static int _lastFallbackRoadmapIndex = -1;

    private static readonly List<GeneratedRoadmap> FallbackRoadmaps = new()
    {
        new GeneratedRoadmap
        {
            Summary = "Dựa trên bài làm gần nhất, lộ trình 3 tuần này giúp bạn củng cố nền tảng, giảm lỗi sai lặp lại và cải thiện các phần còn yếu. Hãy ưu tiên xem lại câu sai, luyện theo từng chủ điểm nhỏ và kiểm tra tiến bộ vào cuối mỗi tuần.",
            Strengths = new List<string>
            {
                "Bạn đã hoàn thành bài luyện tập nên có dữ liệu rõ ràng để xây dựng kế hoạch học.",
                "Có thể xác định được nhóm câu hỏi cần cải thiện từ kết quả bài làm.",
                "Có khả năng tăng điểm nhanh nếu ôn theo từng chủ điểm nhỏ và đều đặn."
            },
            Weaknesses = new List<string>
            {
                "Cần rà soát lại các câu sai và ghi chú lý do sai.",
                "Cần luyện thêm tốc độ làm bài để hạn chế bỏ trống câu hỏi.",
                "Cần ôn tập đều từ vựng, ngữ pháp và đọc hiểu."
            },
            Weeks = new List<StudyRoadmapWeekResult>
            {
                new StudyRoadmapWeekResult
                {
                    Title = "Tuần 1: Sửa lỗi nền tảng",
                    Goal = "Nắm lại các lỗi sai thường gặp trong bài làm gần nhất.",
                    Tasks = new List<string>
                    {
                        "Đọc lại toàn bộ câu sai và viết lý do sai cho từng câu.",
                        "Ôn 2 chủ điểm ngữ pháp yếu nhất trong bài.",
                        "Làm 20 câu trắc nghiệm cùng chủ điểm và chấm lại đáp án."
                    }
                },
                new StudyRoadmapWeekResult
                {
                    Title = "Tuần 2: Tăng tốc độ và độ chính xác",
                    Goal = "Giảm lỗi sai do vội vàng và cải thiện thời gian làm bài.",
                    Tasks = new List<string>
                    {
                        "Làm mỗi ngày một set 15 câu trong 15 phút.",
                        "Ghi lại từ mới và cụm từ hay gặp trong đề.",
                        "Luyện đọc hiểu ngắn, tìm ý chính và từ khóa."
                    }
                },
                new StudyRoadmapWeekResult
                {
                    Title = "Tuần 3: Tổng ôn và thi thử",
                    Goal = "Ôn tổng hợp và kiểm tra mức độ tiến bộ.",
                    Tasks = new List<string>
                    {
                        "Làm lại một đề đầy đủ trong giới hạn thời gian.",
                        "So sánh điểm với bài trước và tìm nhóm lỗi còn lặp lại.",
                        "Chốt danh sách chủ điểm cần ôn trước bài tiếp theo."
                    }
                }
            },
            DailyTime = "35-45 phút mỗi ngày",
            NextAction = "Bắt đầu bằng việc xem lại các câu sai trong bài làm gần nhất."
        },
        new GeneratedRoadmap
        {
            Summary = "Lộ trình này ưu tiên chiến lược tăng điểm ổn định: giữ chắc các câu dễ, giảm lỗi sai lặp lại và cải thiện phần đọc hiểu. Bạn nên học theo từng phần nhỏ, có thời gian cố định mỗi ngày và ghi lại lỗi sau mỗi buổi luyện.",
            Strengths = new List<string>
            {
                "Bạn đã có lịch sử làm bài để làm mốc so sánh tiến bộ.",
                "Có thể cải thiện điểm nhanh bằng cách sửa nhóm lỗi lặp lại.",
                "Việc ôn tập sẽ hiệu quả hơn khi chia bài thành các phần nhỏ."
            },
            Weaknesses = new List<string>
            {
                "Cần hệ thống hóa kiến thức thay vì học rải rác.",
                "Cần luyện thêm phần từ vựng theo ngữ cảnh.",
                "Cần duy trì thời gian ôn tập cố định mỗi ngày."
            },
            Weeks = new List<StudyRoadmapWeekResult>
            {
                new StudyRoadmapWeekResult
                {
                    Title = "Tuần 1: Chắc câu dễ",
                    Goal = "Giữ điểm ở các câu nhận biết và thông hiểu.",
                    Tasks = new List<string>
                    {
                        "Ôn thì, câu bị động, so sánh và câu điều kiện.",
                        "Làm 30 câu mức độ dễ-trung bình và ghi lỗi sai.",
                        "Tạo bảng công thức ngữ pháp cá nhân."
                    }
                },
                new StudyRoadmapWeekResult
                {
                    Title = "Tuần 2: Xử lý từ vựng và đọc hiểu",
                    Goal = "Tăng khả năng suy luận từ ngữ cảnh.",
                    Tasks = new List<string>
                    {
                        "Học 8-10 từ mới mỗi ngày theo chủ đề.",
                        "Đọc 3 đoạn văn ngắn và gạch chân từ khóa.",
                        "Luyện câu hỏi đồng nghĩa, trái nghĩa và điền từ."
                    }
                },
                new StudyRoadmapWeekResult
                {
                    Title = "Tuần 3: Luyện đề có chiến thuật",
                    Goal = "Làm bài có thứ tự ưu tiên và kiểm soát thời gian.",
                    Tasks = new List<string>
                    {
                        "Làm phần dễ chắc trước, đánh dấu câu khó sau.",
                        "Đặt giờ khi làm đề để tạo áp lực như thi thật.",
                        "Tổng kết 5 lỗi sai lớn nhất sau mỗi đề."
                    }
                }
            },
            DailyTime = "40 phút mỗi ngày",
            NextAction = "Chọn 1 chủ điểm ngữ pháp sai nhiều nhất và làm 15 câu luyện tập."
        },
        new GeneratedRoadmap
        {
            Summary = "Lộ trình này tập trung phục hồi điểm yếu từ bài làm gần nhất. Trong 3 tuần, bạn sẽ phân tích lỗi sai, luyện theo nhóm kỹ năng và làm bài kiểm tra lại để đo mức tiến bộ.",
            Strengths = new List<string>
            {
                "Bạn có thể theo dõi tiến bộ qua từng lần làm bài.",
                "Đã có dữ liệu câu đúng, câu sai để xác định mức ưu tiên.",
                "Có thể học hiệu quả nếu ôn theo chu kỳ ngắn mỗi ngày."
            },
            Weaknesses = new List<string>
            {
                "Cần tránh học dồn vào ngày cuối.",
                "Cần luyện kỹ năng đọc đề và loại trừ đáp án.",
                "Cần ôn lại các câu sai thay vì chỉ xem đáp án đúng."
            },
            Weeks = new List<StudyRoadmapWeekResult>
            {
                new StudyRoadmapWeekResult
                {
                    Title = "Tuần 1: Phân tích lỗi sai",
                    Goal = "Biết rõ vì sao từng câu sai và cách tránh lặp lại.",
                    Tasks = new List<string>
                    {
                        "Chia câu sai thành nhóm ngữ pháp, từ vựng, đọc hiểu.",
                        "Viết lại quy tắc đúng cho mỗi câu sai quan trọng.",
                        "Làm lại các câu sai sau 24 giờ."
                    }
                },
                new StudyRoadmapWeekResult
                {
                    Title = "Tuần 2: Luyện theo nhóm kỹ năng",
                    Goal = "Cải thiện nhóm kỹ năng có số câu sai cao nhất.",
                    Tasks = new List<string>
                    {
                        "Mỗi ngày luyện 1 nhóm câu hỏi trong 25 phút.",
                        "Ghi 5 từ/cụm từ mới vào sổ tay.",
                        "Làm bài tập tổng hợp cuối tuần."
                    }
                },
                new StudyRoadmapWeekResult
                {
                    Title = "Tuần 3: Kiểm tra lại",
                    Goal = "Đo lại điểm và điều chỉnh chiến lược ôn tập.",
                    Tasks = new List<string>
                    {
                        "Làm một bài thi thử mới.",
                        "So sánh tỷ lệ sai theo từng phần với bài làm cũ.",
                        "Lập danh sách 3 mục tiêu cho lần luyện tiếp theo."
                    }
                }
            },
            DailyTime = "30-50 phút mỗi ngày",
            NextAction = "Mở lại bài làm gần nhất và gom các câu sai vào 3 nhóm."
        },
        new GeneratedRoadmap
        {
            Summary = "Lộ trình 3 tuần này giúp bạn học đều, chấm lỗi kỹ và luyện đề có giới hạn thời gian. Mục tiêu là biến lỗi sai thành danh sách hành động cụ thể để bạn biết cần ôn gì sau mỗi buổi học.",
            Strengths = new List<string>
            {
                "Bạn đã có động lực cập nhật lộ trình học.",
                "Dữ liệu bài làm giúp xác định mục tiêu ôn tập rõ hơn.",
                "Nếu duy trì tiến độ hằng ngày, điểm có thể cải thiện ổn định."
            },
            Weaknesses = new List<string>
            {
                "Cần tăng tính kỷ luật khi ôn tập mỗi ngày.",
                "Cần luyện cách phân bổ thời gian trong đề.",
                "Cần cụ thể hóa lỗi sai thành hành động ôn tập."
            },
            Weeks = new List<StudyRoadmapWeekResult>
            {
                new StudyRoadmapWeekResult
                {
                    Title = "Tuần 1: Ôn lại kiến thức cốt lõi",
                    Goal = "Lặp lại nền tảng ngữ pháp và từ vựng hay gặp.",
                    Tasks = new List<string>
                    {
                        "Ôn 4 chủ điểm ngữ pháp có tần suất cao.",
                        "Làm flashcard cho từ vựng mới và cụm từ khó.",
                        "Mỗi ngày làm 10 câu để giữ nhịp học."
                    }
                },
                new StudyRoadmapWeekResult
                {
                    Title = "Tuần 2: Thực hành có mục tiêu",
                    Goal = "Giảm số câu sai ở nhóm yếu nhất.",
                    Tasks = new List<string>
                    {
                        "Chọn nhóm câu sai nhiều nhất để luyện riêng.",
                        "Làm bài tập theo cấp độ từ dễ đến khó.",
                        "Sau mỗi bài, ghi lại một mẹo tránh sai."
                    }
                },
                new StudyRoadmapWeekResult
                {
                    Title = "Tuần 3: Về đích",
                    Goal = "Mô phỏng bài thi và chốt chiến thuật làm bài.",
                    Tasks = new List<string>
                    {
                        "Làm 2 đề ngắn có bấm giờ.",
                        "Kiểm tra lại các công thức hay quên.",
                        "Sắp xếp thứ tự làm bài phù hợp với điểm mạnh của bạn."
                    }
                }
            },
            DailyTime = "45 phút mỗi ngày",
            NextAction = "Đặt lịch học 3 ngày đầu tiên và hoàn thành set 10 câu đầu."
        }
    };

    private readonly AppDbContext _dbContext;
    private readonly IGeminiRoadmapService _geminiRoadmapService;
    private readonly ILogger<AiRoadmapService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public AiRoadmapService(
        AppDbContext dbContext,
        IGeminiRoadmapService geminiRoadmapService,
        ILogger<AiRoadmapService> logger)
    {
        _dbContext = dbContext;
        _geminiRoadmapService = geminiRoadmapService;
        _logger = logger;
    }

    public async Task<StudyRoadmapDto?> GetRoadmapAsync(Guid userId)
    {
        var roadmap = await GetExistingRoadmapAsync(userId);
        return roadmap == null ? null : MapToDto(roadmap);
    }

    public async Task<StudyRoadmapDto> GenerateRoadmapAsync(Guid userId)
    {
        var attempt = await GetLatestCompletedAttemptAsync(userId);
        if (attempt == null || attempt.UserAnswers.Count == 0)
        {
            throw new InvalidOperationException("Bạn cần làm bài ít nhất 1 lần để có dữ liệu phân tích");
        }

        var existing = await GetExistingRoadmapAsync(userId);
        if (existing?.UserExamAttemptId == attempt.UserExamAttemptId)
        {
            return MapToDto(existing);
        }

        var generated = await GenerateWithFallbackAsync(attempt);
        var roadmap = existing ?? CreateRoadmap(userId);
        ApplyGeneratedRoadmap(roadmap, attempt, generated);

        await _dbContext.SaveChangesAsync();

        roadmap.UserExamAttempt = attempt;
        return MapToDto(roadmap);
    }

    private async Task<GeneratedRoadmap> GenerateWithFallbackAsync(UserExamAttempt attempt)
    {
        try
        {
            return await _geminiRoadmapService.GenerateRoadmapAsync(attempt);
        }
        catch (Exception ex)
        {
            var fallback = GetNextFallbackRoadmap();
            _logger.LogWarning(
                ex,
                "Gemini roadmap generation failed. Returning fallback roadmap. AttemptId: {AttemptId}",
                attempt.UserExamAttemptId);
            return fallback;
        }
    }

    private static GeneratedRoadmap GetNextFallbackRoadmap()
    {
        lock (FallbackRoadmapLock)
        {
            var nextIndex = Random.Shared.Next(FallbackRoadmaps.Count);
            if (FallbackRoadmaps.Count > 1)
            {
                while (nextIndex == _lastFallbackRoadmapIndex)
                {
                    nextIndex = Random.Shared.Next(FallbackRoadmaps.Count);
                }
            }

            _lastFallbackRoadmapIndex = nextIndex;
            return CloneRoadmap(FallbackRoadmaps[nextIndex]);
        }
    }

    private static GeneratedRoadmap CloneRoadmap(GeneratedRoadmap source)
    {
        return new GeneratedRoadmap
        {
            Summary = source.Summary,
            Strengths = source.Strengths.ToList(),
            Weaknesses = source.Weaknesses.ToList(),
            Weeks = source.Weeks.Select(week => new StudyRoadmapWeekResult
            {
                Title = week.Title,
                Goal = week.Goal,
                Tasks = week.Tasks.ToList()
            }).ToList(),
            DailyTime = source.DailyTime,
            NextAction = source.NextAction
        };
    }

    private async Task<UserStudyRoadmap?> GetExistingRoadmapAsync(Guid userId)
    {
        return await _dbContext.UserStudyRoadmaps
            .Include(item => item.UserExamAttempt)
            .Where(item => item.UserId == userId)
            .OrderByDescending(item => item.UpdatedAt)
            .FirstOrDefaultAsync();
    }

    private async Task<UserExamAttempt?> GetLatestCompletedAttemptAsync(Guid userId)
    {
        return await _dbContext.UserExamAttempts
            .Include(attempt => attempt.Exam)
                .ThenInclude(exam => exam!.Questions)
            .Include(attempt => attempt.Status)
            .Include(attempt => attempt.UserAnswers)
                .ThenInclude(answer => answer.Question)
            .Where(attempt =>
                attempt.UserId == userId &&
                attempt.CompletedAt != null &&
                attempt.Status.Code == AppStatusCodes.Attempts.Submitted)
            .OrderByDescending(attempt => attempt.CompletedAt)
            .FirstOrDefaultAsync();
    }

    private UserStudyRoadmap CreateRoadmap(Guid userId)
    {
        var roadmap = new UserStudyRoadmap
        {
            UserStudyRoadmapId = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.UserStudyRoadmaps.Add(roadmap);
        return roadmap;
    }

    private void ApplyGeneratedRoadmap(
        UserStudyRoadmap roadmap,
        UserExamAttempt attempt,
        GeneratedRoadmap generated)
    {
        roadmap.UserExamAttemptId = attempt.UserExamAttemptId;
        roadmap.Summary = generated.Summary;
        roadmap.Strengths = JsonSerializer.Serialize(generated.Strengths, _jsonOptions);
        roadmap.Weaknesses = JsonSerializer.Serialize(generated.Weaknesses, _jsonOptions);
        roadmap.Weeks = JsonSerializer.Serialize(
            generated.Weeks.Select(week => new StudyRoadmapWeekDto
            {
                Title = week.Title,
                Goal = week.Goal,
                Tasks = week.Tasks
            }).ToList(),
            _jsonOptions);
        roadmap.DailyTime = generated.DailyTime;
        roadmap.NextAction = generated.NextAction;
        roadmap.UpdatedAt = DateTime.UtcNow;
    }

    private StudyRoadmapDto MapToDto(UserStudyRoadmap roadmap)
    {
        return new StudyRoadmapDto
        {
            UserStudyRoadmapId = roadmap.UserStudyRoadmapId,
            SourceAttemptId = roadmap.UserExamAttemptId,
            SourceSubmittedAt = roadmap.UserExamAttempt?.CompletedAt,
            CreatedAt = roadmap.CreatedAt,
            Summary = roadmap.Summary,
            Strengths = DeserializeList(roadmap.Strengths),
            Weaknesses = DeserializeList(roadmap.Weaknesses),
            Weeks = DeserializeWeeks(roadmap.Weeks),
            DailyTime = roadmap.DailyTime,
            NextAction = roadmap.NextAction
        };
    }

    private List<string> DeserializeList(string value)
    {
        return JsonSerializer.Deserialize<List<string>>(value, _jsonOptions) ?? new List<string>();
    }

    private List<StudyRoadmapWeekDto> DeserializeWeeks(string value)
    {
        return JsonSerializer.Deserialize<List<StudyRoadmapWeekDto>>(value, _jsonOptions) ?? new List<StudyRoadmapWeekDto>();
    }
}
