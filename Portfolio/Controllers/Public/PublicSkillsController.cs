using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Common.Extensions;
using Portfolio.Application.Skills.DTOs;
using Portfolio.Application.Skills.Interfaces;
using Portfolio.Application.Common.Models;

namespace Portfolio.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/v1/public/skills")]
[Produces("application/json")]
public sealed class PublicSkillsController
    : ControllerBase
{
    private readonly IPublicSkillService _service;
    private readonly IValidator<PublicSkillFilterRequest>
        _filterValidator;
    private readonly IValidator<PublicLimitRequest>
        _limitValidator;

    public PublicSkillsController(
        IPublicSkillService service,
        IValidator<PublicSkillFilterRequest>
            filterValidator,
        IValidator<PublicLimitRequest>
            limitValidator)
    {
        _service = service;
        _filterValidator = filterValidator;
        _limitValidator = limitValidator;
    }

    [HttpGet("featured")]
    public async Task<
        ActionResult<IReadOnlyList<PublicSkillDto>>>
        GetFeatured(
            [FromQuery] PublicLimitRequest request,
            CancellationToken cancellationToken)
    {
        await _limitValidator.ValidateRequestAsync(
            request,
            cancellationToken);

        return Ok(
            await _service.GetFeaturedAsync(
                request.Limit,
                cancellationToken));
    }

    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<PublicSkillDto>>>
        GetAll(
            [FromQuery]
            PublicSkillFilterRequest filter,
            CancellationToken cancellationToken)
    {
        await _filterValidator.ValidateRequestAsync(
            filter,
            cancellationToken);

        return Ok(
            await _service.GetAllAsync(
                filter,
                cancellationToken));
    }
}

