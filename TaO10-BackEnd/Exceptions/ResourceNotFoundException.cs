namespace TaO10_BackEnd.Exceptions;

/// <summary>
/// Exception thrown when a resource is not found
/// </summary>
public class ResourceNotFoundException : Exception
{
    /// <summary>
    /// Gets the error code
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the ResourceNotFoundException class
    /// </summary>
    public ResourceNotFoundException(string message, string errorCode = "RESOURCE_NOT_FOUND") 
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
