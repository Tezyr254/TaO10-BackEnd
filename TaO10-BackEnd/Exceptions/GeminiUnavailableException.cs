namespace TaO10_BackEnd.Exceptions;

public class GeminiUnavailableException : Exception
{
    public string ErrorCode { get; }

    public GeminiUnavailableException(
        string message = "Gemini đang quá tải, vui lòng thử lại sau ít phút.",
        string errorCode = "GEMINI_UNAVAILABLE")
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
