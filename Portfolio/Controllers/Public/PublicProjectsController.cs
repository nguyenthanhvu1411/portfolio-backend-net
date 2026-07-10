using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Common.Extensions;
using Portfolio.Application.Projects.DTOs;
using Portfolio.Application.Projects.Interfaces;
using Portfolio.Application.Skills.DTOs;

namespace Portfolio.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/v1/public/projects")]
[Produces("application/json")]
public sealed class PublicProjectsController
    : ControllerBase
{
    private readonly IPublicProjectService _service;
    private readonly IValidator<PublicProjectFilterRequest>
        _filterValidator;
    private readonly IValidator<PublicLimitRequest>
        _limitValidator;

    public PublicProjectsController(
        IPublicProjectService service,
        IValidator<PublicProjectFilterRequest>
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
        ActionResult<
            IReadOnlyList<PublicProjectCardDto>>>
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
        ActionResult<
            PagedResult<PublicProjectCardDto>>>
        GetPaged(
            [FromQuery]
            PublicProjectFilterRequest filter,
            CancellationToken cancellationToken)
    {
        await _filterValidator.ValidateRequestAsync(
            filter,
            cancellationToken);

        return Ok(
            await _service.GetPagedAsync(
                filter,
                cancellationToken));
    }

    [HttpGet("{id:int}/images")]
    public async Task<
        ActionResult<
            IReadOnlyList<PublicProjectImageDto>>>
        GetImages(
            int id,
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetImagesAsync(
                id,
                cancellationToken));
    }

    [HttpGet("{slug}")]
    public async Task<
        ActionResult<PublicProjectDetailDto>>
        GetBySlug(
            string slug,
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetBySlugAsync(
                slug,
                cancellationToken));
    }
}

