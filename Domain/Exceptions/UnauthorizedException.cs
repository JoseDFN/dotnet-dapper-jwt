namespace Domain.Exceptions;

public class UnauthorizedException : BaseException
{
    public UnauthorizedException(string message = "Unauthorized access")
        : base(
            statusCode: 401,
            errorCode: "UNAUTHORIZED",
            message: message)
    {
    }

    public UnauthorizedException(string message, object? details)
        : base(
            statusCode: 401,
            errorCode: "UNAUTHORIZED",
            message: message,
            details: details)
    {
    }
}
