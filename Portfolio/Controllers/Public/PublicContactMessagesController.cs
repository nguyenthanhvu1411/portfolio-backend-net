using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Portfolio.Application.Contacts.DTOs;
using Portfolio.Application.Contacts.Interfaces;
using Portfolio.Application.Common.Extensions;
using Portfolio.Application.Common.Security;

namespace Portfolio.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/v1/public/contact-messages")]
[Produces("application/json")]
public sealed class PublicContactMessagesController
    : ControllerBase
{
    private readonly IPublicContactService _service;
    private readonly
        IValidator<
            PublicContactMessageCreateRequest>
        _validator;

    public PublicContactMessagesController(
        IPublicContactService service,
        IValidator<
            PublicContactMessageCreateRequest>
            validator)
    {
        _service = service;
        _validator = validator;
    }

    [HttpPost]
    [EnableRateLimiting(
        PublicRateLimitPolicies.ContactSubmit)]
    [ProducesResponseType(
        typeof(PublicContactMessageDto),
        StatusCodes.Status201Created)]
    public async Task<
        ActionResult<PublicContactMessageDto>>
        Create(
            [FromBody]
            PublicContactMessageCreateRequest request,
            CancellationToken cancellationToken)
    {
        await _validator.ValidateRequestAsync(
            request,
            cancellationToken);

        var result = await _service.CreateAsync(
            request,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }
}

