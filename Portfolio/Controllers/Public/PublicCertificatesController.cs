using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Common.Extensions;
using Portfolio.Application.Resume.DTOs;
using Portfolio.Application.Resume.Interfaces;

namespace Portfolio.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/v1/public/certificates")]
[Produces("application/json")]
public sealed class PublicCertificatesController
    : ControllerBase
{
    private readonly IPublicResumeService _service;
    private readonly
        IValidator<PublicCertificateFilterRequest>
        _validator;

    public PublicCertificatesController(
        IPublicResumeService service,
        IValidator<PublicCertificateFilterRequest>
            validator)
    {
        _service = service;
        _validator = validator;
    }

    [HttpGet]
    public async Task<
        ActionResult<
            IReadOnlyList<PublicCertificateDto>>>
        GetAll(
            [FromQuery]
            PublicCertificateFilterRequest filter,
            CancellationToken cancellationToken)
    {
        await _validator.ValidateRequestAsync(
            filter,
            cancellationToken);

        return Ok(
            await _service.GetCertificatesAsync(
                filter,
                cancellationToken));
    }
}


