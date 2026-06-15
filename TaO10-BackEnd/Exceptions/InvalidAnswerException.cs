namespace TaO10_BackEnd.Exceptions;

/// <summary>
/// Exception thrown when an invalid answer is provided
/// </summary>
public class InvalidAnswerException : Exception
{
    /// <summary>
    /// Gets the error code
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the InvalidAnswerException class
    /// </summary>
    public InvalidAnswerException(string message, string errorCode = "INVALID_ANSWER")
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
