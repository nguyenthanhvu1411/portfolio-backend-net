using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Application.Auth.DTOs;
using Portfolio.Application.Auth.Interfaces;
using Portfolio.Application.Common.Extensions;
using Portfolio.Application.Common.Security;

namespace Portfolio.Controllers.Admin;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName =
        "portfolio_refresh_token";

    private const string RefreshTokenCookiePath =
        "/api/auth";

    private readonly IAuthService _authService;
    private readonly IValidator<AdminLoginRequest> _loginValidator;

    public AuthController(
        IAuthService authService,
        IValidator<AdminLoginRequest> loginValidator)
    {
        _authService = authService;
        _loginValidator = loginValidator;
    }

    /// <summary>
    /// Đăng nhập tài khoản quản trị.
    /// </summary>
    [AllowAnonymous]
    [EnableRateLimiting(AuthPolicies.LoginRateLimit)]
    [HttpPost("login")]
    [ProducesResponseType(
        typeof(AdminLoginResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AdminLoginResponse>> Login(
        [FromBody] AdminLoginRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult =
            await _loginValidator.ValidateAsync(
                request,
                cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(
                CreateValidationProblem(
                    validationResult.ToValidationDictionary()));
        }

        var session = await _authService.LoginAsync(
            request,
            cancellationToken);

        if (session is null)
        {
            return Unauthorized(
                CreateProblem(
                    StatusCodes.Status401Unauthorized,
                    "Đăng nhập thất bại",
                    "Email hoặc mật khẩu không đúng.",
                    "https://httpstatuses.com/401"));
        }

        AppendRefreshTokenCookie(
            session.RefreshToken,
            session.RefreshTokenExpiresAt);

        var response = new AdminLoginResponse
        {
            AccessToken = session.AccessToken.Token,
            AccessTokenExpiresAt =
                session.AccessToken.ExpiresAt,
            Admin = session.Admin
        };

        return Ok(response);
    }

    /// <summary>
    /// Cấp lại access token bằng refresh token trong HttpOnly cookie.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(
        typeof(AdminLoginResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AdminLoginResponse>> Refresh(
        CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(
                RefreshTokenCookieName,
                out var refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(
                CreateProblem(
                    StatusCodes.Status401Unauthorized,
                    "Phiên đăng nhập không hợp lệ",
                    "Refresh token không tồn tại hoặc đã hết hạn.",
                    "https://httpstatuses.com/401"));
        }

        var session = await _authService.RefreshAsync(
            refreshToken,
            cancellationToken);

        if (session is null)
        {
            DeleteRefreshTokenCookie();

            return Unauthorized(
                CreateProblem(
                    StatusCodes.Status401Unauthorized,
                    "Phiên đăng nhập không hợp lệ",
                    "Refresh token không hợp lệ, đã bị thu hồi hoặc đã hết hạn.",
                    "https://httpstatuses.com/401"));
        }

        // Rotation: thay refresh token cũ bằng refresh token mới.
        AppendRefreshTokenCookie(
            session.RefreshToken,
            session.RefreshTokenExpiresAt);

        var response = new AdminLoginResponse
        {
            AccessToken = session.AccessToken.Token,
            AccessTokenExpiresAt =
                session.AccessToken.ExpiresAt,
            Admin = session.Admin
        };

        return Ok(response);
    }

    /// <summary>
    /// Đăng xuất tài khoản quản trị.
    /// </summary>
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken)
    {
        Request.Cookies.TryGetValue(
            RefreshTokenCookieName,
            out var refreshToken);

        await _authService.LogoutAsync(
            refreshToken,
            cancellationToken);

        DeleteRefreshTokenCookie();

        return NoContent();
    }

    /// <summary>
    /// Lấy thông tin tài khoản quản trị đang đăng nhập.
    /// </summary>
    [Authorize(Policy = AuthPolicies.AdminOnly)]
    [HttpGet("me")]
    [ProducesResponseType(
        typeof(CurrentAdminResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentAdminResponse>> Me(
        CancellationToken cancellationToken)
    {
        var userIdValue =
            User.FindFirstValue(
                JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId) ||
            userId <= 0)
        {
            return Unauthorized(
                CreateProblem(
                    StatusCodes.Status401Unauthorized,
                    "Access token không hợp lệ",
                    "Không xác định được người dùng từ access token.",
                    "https://httpstatuses.com/401"));
        }

        var admin =
            await _authService.GetCurrentAdminAsync(
                userId,
                cancellationToken);

        if (admin is null)
        {
            return Unauthorized(
                CreateProblem(
                    StatusCodes.Status401Unauthorized,
                    "Tài khoản không hợp lệ",
                    "Tài khoản không tồn tại, đã bị khóa hoặc không có quyền quản trị.",
                    "https://httpstatuses.com/401"));
        }

        return Ok(admin);
    }

    private void AppendRefreshTokenCookie(
        string refreshToken,
        DateTime expiresAtUtc)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            refreshToken);

        var utcExpiry = DateTime.SpecifyKind(
            expiresAtUtc,
            DateTimeKind.Utc);

        Response.Cookies.Append(
            RefreshTokenCookieName,
            refreshToken,
            CreateRefreshTokenCookieOptions(
                new DateTimeOffset(utcExpiry)));
    }

    private void DeleteRefreshTokenCookie()
    {
        Response.Cookies.Delete(
            RefreshTokenCookieName,
            CreateRefreshTokenCookieOptions());
    }

    private static CookieOptions
        CreateRefreshTokenCookieOptions(
            DateTimeOffset? expires = null)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            Path = RefreshTokenCookiePath,
            Expires = expires
        };
    }

    private ValidationProblemDetails
        CreateValidationProblem(
            IDictionary<string, string[]> errors)
    {
        var problem =
            new ValidationProblemDetails(errors)
            {
                Status =
                    StatusCodes.Status400BadRequest,
                Title = "Dữ liệu không hợp lệ",
                Detail =
                    "Vui lòng kiểm tra lại các trường được gửi lên.",
                Type =
                    "https://httpstatuses.com/400",
                Instance =
                    HttpContext.Request.Path
            };

        problem.Extensions["traceId"] =
            HttpContext.TraceIdentifier;

        return problem;
    }

    private ProblemDetails CreateProblem(
        int status,
        string title,
        string detail,
        string type)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = type,
            Instance = HttpContext.Request.Path
        };

        problem.Extensions["traceId"] =
            HttpContext.TraceIdentifier;

        return problem;
    }
}