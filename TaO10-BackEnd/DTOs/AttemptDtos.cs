using System;
using System.Collections.Generic;
using TaO10_BackEnd.DTOs.Exam;

namespace TaO10_BackEnd.DTOs.Attempt
{
    public class StartAttemptResponse
    {
        public Guid UserExamAttemptId { get; set; }
        public string ExamTitle { get; set; } = null!;
        public int DurationTime { get; set; }
        public List<QuestionResponse> Questions { get; set; } = new();
    }

    public class SaveAnswerRequest
    {
        public Guid QuestionId { get; set; }
        public string UserAnswer { get; set; } = null!; // A, B, C, D
    }

    public class SubmitAttemptRequest
    {
        public List<SaveAnswerRequest> Answers { get; set; } = new();
    }

    public class QuestionResultDto
    {
        public Guid QuestionId { get; set; }
        public int QuestionNumber { get; set; }
        public string? Section { get; set; }
        public string QuestionText { get; set; } = null!;
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? UserAnswer { get; set; }
        public string? CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public string? Explanation { get; set; }
        public decimal? Points { get; set; }
    }

    public class SubmitAttemptResponse
    {
        public Guid UserExamAttemptId { get; set; }
        public decimal Score { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public int TimeSpentMinutes { get; set; }
        public List<QuestionResultDto> Details { get; set; } = new();
    }

    public class AttemptHistoryResponse
    {
        public Guid UserExamAttemptId { get; set; }
        public Guid ExamId { get; set; }
        public string ExamTitle { get; set; } = null!;
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public decimal? Score { get; set; }
        public int? CorrectAnswers { get; set; }
        public int? TotalQuestions { get; set; }
        public int? TimeSpentMinutes { get; set; }
        public string Status { get; set; } = null!;
    }
}
