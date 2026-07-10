using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Blogs.DTOs;
using Portfolio.Application.Blogs.Interfaces;
using Portfolio.Application.Common.Extensions;
using Portfolio.Application.Common.Security;

namespace Portfolio.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/v1/public/blogs")]
[Produces("application/json")]
public sealed class PublicBlogsController
    : ControllerBase
{
    private readonly IPublicBlogService _service;
    private readonly IValidator<PublicBlogFilterRequest>
        _validator;

    public PublicBlogsController(
        IPublicBlogService service,
        IValidator<PublicBlogFilterRequest>
            validator)
    {
        _service = service;
        _validator = validator;
    }

    [HttpGet]
    public async Task<
        ActionResult<
            PagedResult<PublicBlogCardDto>>>
        GetPaged(
            [FromQuery]
            PublicBlogFilterRequest filter,
            CancellationToken cancellationToken)
    {
        await _validator.ValidateRequestAsync(
            filter,
            cancellationToken);

        return Ok(
            await _service.GetPagedAsync(
                filter,
                cancellationToken));
    }

    [HttpGet("{slug}")]
    public async Task<
        ActionResult<PublicBlogDetailDto>>
        GetBySlug(
            string slug,
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetBySlugAsync(
                slug,
                cancellationToken));
    }

    [HttpPost("{id:int}/increase-view")]
    [EnableRateLimiting(
        PublicRateLimitPolicies.BlogView)]
    public async Task<ActionResult<OperationResult>>
        IncreaseView(
            int id,
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service.IncreaseViewAsync(
                id,
                cancellationToken));
    }
}

