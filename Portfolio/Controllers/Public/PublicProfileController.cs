using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Profiles.DTOs;
using Portfolio.Application.Profiles.Interfaces;

namespace Portfolio.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/v1/public/profile")]
[Produces("application/json")]
public sealed class PublicProfileController
    : ControllerBase
{
    private readonly IPublicProfileService _service;

    public PublicProfileController(
        IPublicProfileService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(PublicProfileDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicProfileDto>> Get(
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetAsync(
                cancellationToken));
    }

    [HttpGet("cv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status302Found)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadCv(
        CancellationToken cancellationToken)
    {
        var cv = await _service.GetCvAsync(
            cancellationToken);

        if (!string.IsNullOrWhiteSpace(
                cv.PhysicalPath))
        {
            return PhysicalFile(
                cv.PhysicalPath,
                cv.ContentType,
                cv.DownloadFileName,
                enableRangeProcessing: true);
        }

        return Redirect(cv.FileUrl);
    }
}

