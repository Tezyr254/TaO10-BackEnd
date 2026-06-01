namespace TaO10_BackEnd.Exceptions;

/// <summary>
/// Exception thrown when an exam attempt has already been completed
/// </summary>
public class ExamAlreadyCompletedException : Exception
{
    /// <summary>
    /// Gets the error code
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the ExamAlreadyCompletedException class
    /// </summary>
    public ExamAlreadyCompletedException(string message, string errorCode = "EXAM_ALREADY_COMPLETED")
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
