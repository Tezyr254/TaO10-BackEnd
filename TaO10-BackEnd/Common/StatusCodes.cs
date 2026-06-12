namespace TaO10_BackEnd.Common;

public static class AppStatusCodes
{
    public static class EntityTypes
    {
        public const string Exam = "Exam";
        public const string Question = "Question";
        public const string UserExamAttempt = "UserExamAttempt";
    }

    public static class Exams
    {
        public const string Active = "ACTIVE";
        public const string Inactive = "INACTIVE";
    }

    public static class Questions
    {
        public const string Active = "ACTIVE";
        public const string Deleted = "DELETED";
    }

    public static class Attempts
    {
        public const string InProgress = "ACTIVE";
        public const string Submitted = "COMPLETED";
    }
}
