namespace Domain.Exceptions;

public abstract class BaseException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }
    public object? Details { get; }

    protected BaseException(
        int statusCode,
        string errorCode,
        string message,
        object? details = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Details = details;
    }
}
