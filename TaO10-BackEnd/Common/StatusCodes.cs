namespace TaO10_BackEnd.Common;

public static class AppStatusCodes
{
    public static class EntityTypes
    {
        public const string Exam = "EXAM";
        public const string Question = "QUESTION";
    }

    public static class Exams
    {
        public const string Draft = "DRAFT";
        public const string Published = "PUBLISHED";
        public const string Archived = "ARCHIVED";
        public const string Deleted = "DELETED";
    }

    public static class Questions
    {
        public const string Active = "ACTIVE";
        public const string Deleted = "DELETED";
    }

    public static class Attempts
    {
        public const string InProgress = "IN_PROGRESS";
        public const string Submitted = "SUBMITTED";
    }
}
