namespace TaO10_BackEnd.Exceptions;

public class GeminiQuotaExceededException : Exception
{
    public string ErrorCode { get; }

    public GeminiQuotaExceededException(
        string message = "Gemini đang giới hạn lượt gọi, vui lòng thử lại sau ít phút.",
        string errorCode = "GEMINI_QUOTA_EXCEEDED")
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
