using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Resume.DTOs;
using Portfolio.Application.Resume.Interfaces;

namespace Portfolio.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/v1/public/experiences")]
[Produces("application/json")]
public sealed class PublicExperiencesController
    : ControllerBase
{
    private readonly IPublicResumeService _service;

    public PublicExperiencesController(
        IPublicResumeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<
        ActionResult<
            IReadOnlyList<PublicExperienceDto>>>
        GetAll(
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetExperiencesAsync(
                cancellationToken));
    }
}

