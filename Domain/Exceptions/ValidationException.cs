namespace Domain.Exceptions;

public class ValidationException : BaseException
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(Dictionary<string, string[]> errors)
        : base(
            statusCode: 400,
            errorCode: "VALIDATION_ERROR",
            message: "One or more validation errors occurred",
            details: errors)
    {
        Errors = errors;
    }

    public ValidationException(string field, string errorMessage)
        : base(
            statusCode: 400,
            errorCode: "VALIDATION_ERROR",
            message: "One or more validation errors occurred",
            details: new Dictionary<string, string[]> { { field, new[] { errorMessage } } })
    {
        Errors = new Dictionary<string, string[]> { { field, new[] { errorMessage } } };
    }
}
