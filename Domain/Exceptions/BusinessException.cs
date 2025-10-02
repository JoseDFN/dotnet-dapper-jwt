namespace Domain.Exceptions;

public class BusinessException : BaseException
{
    public BusinessException(string message, object? details = null)
        : base(
            statusCode: 422,
            errorCode: "BUSINESS_RULE_VIOLATION",
            message: message,
            details: details)
    {
    }

    public BusinessException(string errorCode, string message, object? details = null)
        : base(
            statusCode: 422,
            errorCode: errorCode,
            message: message,
            details: details)
    {
    }
}
