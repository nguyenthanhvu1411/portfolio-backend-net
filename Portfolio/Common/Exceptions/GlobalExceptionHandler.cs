using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Portfolio.Common.Exceptions;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception. TraceId: {TraceId}",
            httpContext.TraceIdentifier);

        ProblemDetails problem;

        switch (exception)
        {
            case RequestValidationException validationException:
                problem = new ValidationProblemDetails(
                    validationException.Errors.ToDictionary(k => k.Key, k => k.Value))
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Dữ liệu không hợp lệ",
                    Detail = validationException.Message,
                    Type = "https://httpstatuses.com/400",
                    Instance = httpContext.Request.Path
                };
                break;

            case ConflictException:
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Dữ liệu xung đột",
                    Detail = exception.Message,
                    Type = "https://httpstatuses.com/409",
                    Instance = httpContext.Request.Path
                };
                break;

            case NotFoundException:
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Không tìm thấy dữ liệu",
                    Detail = exception.Message,
                    Type = "https://httpstatuses.com/404",
                    Instance = httpContext.Request.Path
                };
                break;

            case UnauthorizedAccessException:
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Không có quyền truy cập",
                    Detail = exception.Message,
                    Type = "https://httpstatuses.com/401",
                    Instance = httpContext.Request.Path
                };
                break;

            default:
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Lỗi hệ thống",
                    Detail = _environment.IsDevelopment()
                        ? exception.GetBaseException().Message
                        : "Đã xảy ra lỗi trong quá trình xử lý yêu cầu.",
                    Type = "https://httpstatuses.com/500",
                    Instance = httpContext.Request.Path
                };
                break;
        }

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode =
            problem.Status ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        return true;
    }
}
