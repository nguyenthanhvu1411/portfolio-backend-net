using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Common.Extensions;
using Portfolio.Application.Common.Security;
using Portfolio.Application.Profiles.DTOs;
using Portfolio.Application.Profiles.Interfaces;

namespace Portfolio.Controllers.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminOnly)]
[Route("api/v1/admin/profile")]
[Produces("application/json")]
public sealed class AdminProfileController : AdminControllerBase
{
    private const long UploadRequestLimit = 12 * 1024 * 1024;

    private readonly IAdminProfileService _profileService;
    private readonly IValidator<ProfileUpdateRequest> _updateValidator;

    public AdminProfileController(
        IAdminProfileService profileService,
        IValidator<ProfileUpdateRequest> updateValidator)
    {
        _profileService = profileService;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileDto>> Get(
        CancellationToken cancellationToken)
    {
        return Ok(await _profileService.GetAsync(cancellationToken));
    }

    [HttpPut]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProfileDto>> Update(
        [FromBody] ProfileUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(
            request,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return ValidationProblem(
                new ValidationProblemDetails(
                    validationResult.ToValidationDictionary())
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Dá»¯ liá»‡u khÃ´ng há»£p lá»‡",
                    Detail = "Vui lÃ²ng kiá»ƒm tra láº¡i thÃ´ng tin profile.",
                    Type = "https://httpstatuses.com/400",
                    Instance = HttpContext.Request.Path
                });
        }

        var result = await _profileService.UpdateAsync(
            request,
            GetCurrentUserId(),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("avatar")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(UploadRequestLimit)]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FileUploadResponse>> UploadAvatar(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        return Ok(await _profileService.UploadAvatarAsync(
            file,
            GetCurrentUserId(),
            cancellationToken));
    }

    [HttpDelete("avatar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAvatar(
        CancellationToken cancellationToken)
    {
        await _profileService.DeleteAvatarAsync(
            GetCurrentUserId(),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("banner")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(UploadRequestLimit)]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FileUploadResponse>> UploadBanner(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        return Ok(await _profileService.UploadBannerAsync(
            file,
            GetCurrentUserId(),
            cancellationToken));
    }

    [HttpDelete("banner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteBanner(
        CancellationToken cancellationToken)
    {
        await _profileService.DeleteBannerAsync(
            GetCurrentUserId(),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("cv")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(UploadRequestLimit)]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FileUploadResponse>> UploadCv(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        return Ok(await _profileService.UploadCvAsync(
            file,
            GetCurrentUserId(),
            cancellationToken));
    }

    [HttpDelete("cv")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteCv(
        CancellationToken cancellationToken)
    {
        await _profileService.DeleteCvAsync(
            GetCurrentUserId(),
            cancellationToken);

        return NoContent();
    }
}

