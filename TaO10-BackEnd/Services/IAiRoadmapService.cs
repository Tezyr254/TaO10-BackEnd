using TaO10_BackEnd.DTOs.AiRoadmaps;

namespace TaO10_BackEnd.Services;

public interface IAiRoadmapService
{
    Task<StudyRoadmapDto?> GetRoadmapAsync(Guid userId);

    Task<StudyRoadmapDto> GenerateRoadmapAsync(Guid userId);
}
