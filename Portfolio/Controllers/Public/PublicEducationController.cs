using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Resume.DTOs;
using Portfolio.Application.Resume.Interfaces;

namespace Portfolio.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/v1/public/education")]
[Produces("application/json")]
public sealed class PublicEducationController
    : ControllerBase
{
    private readonly IPublicResumeService _service;

    public PublicEducationController(
        IPublicResumeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<
        ActionResult<
            IReadOnlyList<PublicEducationDto>>>
        GetAll(
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetEducationAsync(
                cancellationToken));
    }
}

