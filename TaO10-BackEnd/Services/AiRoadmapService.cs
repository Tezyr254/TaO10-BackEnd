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
            Summary = "AI hien chua san sang, he thong tam tao lo trinh on tap 3 tuan dua tren bai lam gan nhat cua ban. Tap trung uu tien cac cau sai, cau bo trong va nhung phan diem con thap.",
            Strengths = new List<string>
            {
                "Da hoan thanh bai luyen tap nen co du lieu de xay dung ke hoach hoc.",
                "Co nen tang de nhan dien nhom cau hoi can cai thien.",
                "Co the tang diem nhanh neu on theo tung chu diem nho."
            },
            Weaknesses = new List<string>
            {
                "Can ra soat lai cac cau sai va ghi chu ly do sai.",
                "Can luyen them toc do lam bai de han che bo trong cau hoi.",
                "Can on tap deu tu vung, ngu phap va doc hieu."
            },
            Weeks = new List<StudyRoadmapWeekResult>
            {
                new StudyRoadmapWeekResult
                {
                    Title = "Tuan 1: Sua loi nen tang",
                    Goal = "Nam lai cac loi sai thuong gap trong bai lam gan nhat.",
                    Tasks = new List<string>
                    {
                        "Doc lai toan bo cau sai va viet ly do sai cho tung cau.",
                        "On 2 chu diem ngu phap yeu nhat trong bai.",
                        "Lam 20 cau trac nghiem cung chu diem va cham lai dap an."
                    }
                },
                new StudyRoadmapWeekResult
                {
                    Title = "Tuan 2: Tang toc do va do chinh xac",
                    Goal = "Giam loi sai do vo y va cai thien thoi gian lam bai.",
                    Tasks = new List<string>
                    {
                        "Lam moi ngay mot set 15 cau trong 15 phut.",
                        "Ghi lai tu moi va cum tu hay gap trong de.",
                        "Luyen doc hieu ngan, tim y chinh va tu khoa."
                    }
                },
                new StudyRoadmapWeekResult
                {
                    Title = "Tuan 3: Tong on va thi thu",
                    Goal = "On tong hop va kiem tra muc do tien bo.",
                    Tasks = new List<string>
                    {
                        "Lam lai mot de day du trong gioi han thoi gian.",
                        "So sanh diem voi bai truoc va tim nhom loi con lap lai.",
                        "Chot danh sach chu diem can on truoc bai tiep theo."
                    }
                }
            },
            DailyTime = "35-45 phut moi ngay",
            NextAction = "Bat dau bang viec xem lai cac cau sai trong attempt gan nhat."
        },
        new GeneratedRoadmap
        {
            Summary = "Day la lo trinh du phong khi dich vu AI tam thoi khong phan hoi. Ke hoach uu tien on tap theo chien luoc diem so: chac cau de, giam loi sai va cai thien doc hieu.",
            Strengths = new List<string>
            {
                "Ban da co lich su lam bai de lam moc so sanh tien bo.",
                "Co the cai thien diem nhanh bang cach sua nhom loi lap lai.",
                "Kha nang on tap se tot hon khi chia bai thanh cac phan nho."
            },
            Weaknesses = new List<string>
            {
                "Can he thong hoa kien thuc thay vi hoc rai rac.",
                "Can luyen them phan tu vung theo ngu canh.",
                "Can duy tri thoi gian on tap co dinh moi ngay."
            },
            Weeks = new List<StudyRoadmapWeekResult>
            {
                new StudyRoadmapWeekResult
                {
                    Title = "Tuan 1: Chac cau de",
                    Goal = "Giu diem o cac cau nhan biet va thong hieu.",
                    Tasks = new List<string>
                    {
                        "On thi, cau bi dong, so sanh va cau dieu kien.",
                        "Lam 30 cau muc do de-trung binh va ghi loi sai.",
                        "Tao bang cong thuc ngu phap ca nhan."
                    }
                },
                new StudyRoadmapWeekResult
                {
                    Title = "Tuan 2: Xu ly tu vung va doc hieu",
                    Goal = "Tang kha nang suy luan tu ngu canh.",
                    Tasks = new List<string>
                    {
                        "Hoc 8-10 tu moi moi ngay theo chu de.",
                        "Doc 3 doan van ngan va gach chan tu khoa.",
                        "Luyen cau hoi dong nghia, trai nghia va dien tu."
                    }
                },
                new StudyRoadmapWeekResult
                {
                    Title = "Tuan 3: Luyen de co chien thuat",
                    Goal = "Lam bai co thu tu uu tien va kiem soat thoi gian.",
                    Tasks = new List<string>
                    {
                        "Lam phan de chac truoc, danh dau cau kho sau.",
                        "Dat gio khi lam de de tao ap luc nhu thi that.",
                        "Tong ket 5 loi sai lon nhat sau moi de."
                    }
                }
            },
            DailyTime = "40 phut moi ngay",
            NextAction = "Chon 1 chu diem ngu phap sai nhieu nhat va lam 15 cau luyen tap."
        },
        new GeneratedRoadmap
        {
            Summary = "AI dang gap loi ket noi nen he thong tao ke hoach mau theo huong phuc hoi diem yeu. Lo trinh nay phu hop khi ban muon on lai tu bai lam gan nhat va tang diem on dinh.",
            Strengths = new List<string>
            {
                "Ban co the theo doi tien bo qua tung lan lam bai.",
                "Da co du lieu cau dung, cau sai de xac dinh muc uu tien.",
                "Co the hoc hieu qua neu on theo chu ky ngan moi ngay."
            },
            Weaknesses = new List<string>
            {
                "Can tranh hoc don don vao ngay cuoi.",
                "Can luyen ky nang doc de va loai tru dap an.",
                "Can on lai cac cau sai thay vi chi xem dap an dung."
            },
            Weeks = new List<StudyRoadmapWeekResult>
            {
                new StudyRoadmapWeekResult
                {
                    Title = "Tuan 1: Phan tich loi sai",
                    Goal = "Biet ro vi sao tung cau sai va cach tranh lap lai.",
                    Tasks = new List<string>
                    {
                        "Chia cau sai thanh nhom ngu phap, tu vung, doc hieu.",
                        "Viet lai quy tac dung cho moi cau sai quan trong.",
                        "Lam lai cac cau sai sau 24 gio."
                    }
                },
                new StudyRoadmapWeekResult
                {
                    Title = "Tuan 2: Luyen theo nhom ky nang",
                    Goal = "Cai thien nhom ky nang co so cau sai cao nhat.",
                    Tasks = new List<string>
                    {
                        "Moi ngay luyen 1 nhom cau hoi trong 25 phut.",
                        "Ghi 5 tu/cum tu moi vao so tay.",
                        "Lam bai tap tong hop cuoi tuan."
                    }
                },
                new StudyRoadmapWeekResult
                {
                    Title = "Tuan 3: Kiem tra lai",
                    Goal = "Do lai diem va dieu chinh chien luoc on tap.",
                    Tasks = new List<string>
                    {
                        "Lam mot bai thi thu moi.",
                        "So sanh ty le sai theo tung phan voi attempt cu.",
                        "Lap danh sach 3 muc tieu cho lan luyen tiep theo."
                    }
                }
            },
            DailyTime = "30-50 phut moi ngay",
            NextAction = "Mo lai bai lam gan nhat va gom cac cau sai vao 3 nhom."
        },
        new GeneratedRoadmap
        {
            Summary = "Do Gemini chua phan hoi thanh cong, day la lo trinh du phong de ban van co ke hoach hoc ngay. Trong 3 tuan, hay uu tien hoc deu, cham loi ky va lam de co gioi han thoi gian.",
            Strengths = new List<string>
            {
                "Ban da co dong luc cap nhat lo trinh hoc.",
                "Du lieu attempt giup xac dinh muc tieu on tap ro hon.",
                "Neu duy tri tien do hang ngay, diem co the cai thien on dinh."
            },
            Weaknesses = new List<string>
            {
                "Can tang tinh ky luat khi on tap moi ngay.",
                "Can luyen cach phan bo thoi gian trong de.",
                "Can cu the hoa loi sai thanh hanh dong on tap."
            },
            Weeks = new List<StudyRoadmapWeekResult>
            {
                new StudyRoadmapWeekResult
                {
                    Title = "Tuan 1: On lai kien thuc cot loi",
                    Goal = "Lap lai nen tang ngu phap va tu vung hay gap.",
                    Tasks = new List<string>
                    {
                        "On 4 chu diem ngu phap co tan suat cao.",
                        "Lam flashcard cho tu vung moi va cum tu kho.",
                        "Moi ngay lam 10 cau de giu nhip hoc."
                    }
                },
                new StudyRoadmapWeekResult
                {
                    Title = "Tuan 2: Thuc hanh co muc tieu",
                    Goal = "Giam so cau sai o nhom yeu nhat.",
                    Tasks = new List<string>
                    {
                        "Chon nhom cau sai nhieu nhat de luyen rieng.",
                        "Lam bai tap theo cap do tu de den kho.",
                        "Sau moi bai, ghi lai mot meo tranh sai."
                    }
                },
                new StudyRoadmapWeekResult
                {
                    Title = "Tuan 3: Ve dich",
                    Goal = "Mo phong bai thi va chot chien thuat lam bai.",
                    Tasks = new List<string>
                    {
                        "Lam 2 de ngan co bam gio.",
                        "Kiem tra lai cac cong thuc hay quen.",
                        "Sap xep thu tu lam bai phu hop voi diem manh cua ban."
                    }
                }
            },
            DailyTime = "45 phut moi ngay",
            NextAction = "Dat lich hoc 3 ngay dau tien va hoan thanh set 10 cau dau."
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
