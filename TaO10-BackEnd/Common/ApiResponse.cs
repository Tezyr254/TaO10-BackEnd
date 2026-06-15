namespace TaO10_BackEnd.Common;

/// <summary>
/// Wrapper for API responses
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The response data
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Operation message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Error code (if operation failed)
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Creates a success response
    /// </summary>
    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    public static ApiResponse<T> ErrorResponse(string message, string? errorCode = null, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            Message = message,
            ErrorCode = errorCode,
            StatusCode = statusCode
        };
    }
}

/// <summary>
/// Wrapper for API responses without data
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Operation message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Error code (if operation failed)
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Creates a success response
    /// </summary>
    public static ApiResponse SuccessResponse(string message = "Operation successful", int statusCode = 200)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message,
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    public static ApiResponse ErrorResponse(string message, string? errorCode = null, int statusCode = 400)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            ErrorCode = errorCode,
            StatusCode = statusCode
        };
    }
}
