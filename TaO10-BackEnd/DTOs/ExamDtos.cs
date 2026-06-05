using System;
using System.Collections.Generic;

namespace TaO10_BackEnd.DTOs.Exam
{
    public class ExamResponse
    {
        public Guid ExamId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? QuestionsCount { get; set; }
        public int DurationTime { get; set; }
        public string? Level { get; set; }
        public int? Year { get; set; }
        public string? ExamType { get; set; }
        public int? ViewsCount { get; set; }
        public int? AttemptsCount { get; set; }
        public string Status { get; set; } = null!;
        public bool IsPremium { get; set; }
        public int? ProgressPercentage { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class CreateExamRequest
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int DurationTime { get; set; }
        public string? Level { get; set; }
        public int? Year { get; set; }
        public string? ExamType { get; set; }
        public bool IsPremium { get; set; }
    }

    public class UpdateExamRequest : CreateExamRequest
    {
        public string Status { get; set; } = null!;
    }

    public class QuestionResponse
    {
        public Guid QuestionId { get; set; }
        public int QuestionNumber { get; set; }
        public string? Section { get; set; }
        public string QuestionText { get; set; } = null!;
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public decimal? Points { get; set; }
    }

    public class QuestionAdminResponse : QuestionResponse
    {
        public string? CorrectAnswer { get; set; }
        public string? Explanation { get; set; }
        public string Status { get; set; } = null!;
    }

    public class CreateQuestionRequest
    {
        public Guid ExamId { get; set; }
        public int QuestionNumber { get; set; }
        public string? Section { get; set; }
        public string QuestionText { get; set; } = null!;
        public string? OptionA { get; set; }
        public string? OptionB { get; set; }
        public string? OptionC { get; set; }
        public string? OptionD { get; set; }
        public string? CorrectAnswer { get; set; }
        public string? Explanation { get; set; }
        public decimal Points { get; set; } = 1.0m;
    }

    public class UpdateQuestionRequest : CreateQuestionRequest
    {
        public string Status { get; set; } = null!;
    }
}
