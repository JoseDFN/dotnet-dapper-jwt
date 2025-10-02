namespace Domain.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string resourceName, object key)
        : base(
            statusCode: 404,
            errorCode: "RESOURCE_NOT_FOUND",
            message: $"{resourceName} with key '{key}' was not found",
            details: new { ResourceName = resourceName, Key = key })
    {
    }

    public NotFoundException(string resourceName, object key, string customMessage)
        : base(
            statusCode: 404,
            errorCode: "RESOURCE_NOT_FOUND",
            message: customMessage,
            details: new { ResourceName = resourceName, Key = key })
    {
    }
}
