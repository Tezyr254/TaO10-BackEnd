namespace TaO10_BackEnd.Exceptions;

/// <summary>
/// Exception thrown when access to an exam is denied
/// </summary>
public class ExamAccessDeniedException : Exception
{
    /// <summary>
    /// Gets the error code
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the ExamAccessDeniedException class
    /// </summary>
    public ExamAccessDeniedException(string message, string errorCode = "EXAM_ACCESS_DENIED")
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
