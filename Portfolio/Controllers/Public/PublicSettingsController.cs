using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Settings.DTOs;
using Portfolio.Application.Settings.Interfaces;

namespace Portfolio.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/v1/public/settings")]
[Produces("application/json")]
public sealed class PublicSettingsController
    : ControllerBase
{
    private readonly IPublicSettingService _service;

    public PublicSettingsController(
        IPublicSettingService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(PublicSettingDto),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<PublicSettingDto>> Get(
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetAsync(
                cancellationToken));
    }
}

