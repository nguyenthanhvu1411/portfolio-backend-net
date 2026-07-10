namespace Portfolio.Common.Exceptions;

public sealed class RequestValidationException : Exception
{
    public RequestValidationException(
        string field,
        string message)
        : this(new Dictionary<string, string[]>
        {
            [field] = [message]
        })
    {
    }

    public RequestValidationException(
        IDictionary<string, string[]> errors)
        : base("Dữ liệu yêu cầu không hợp lệ.")
    {
        Errors = new Dictionary<string, string[]>(
            errors,
            StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
