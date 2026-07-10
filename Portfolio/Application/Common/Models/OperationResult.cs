namespace Portfolio.Application.Common.Models;

public sealed class OperationResult
{
    public bool Success { get; init; } = true;
    public string Message { get; init; } = string.Empty;
}
