using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Certificates.DTOs;
using Portfolio.Application.Certificates.Interfaces;
using Portfolio.Application.Common.Extensions;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Common.Security;
using Portfolio.Common.Exceptions;

namespace Portfolio.Controllers.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminOnly)]
[Route("api/v1/admin/certificates")]
[Produces("application/json")]
public sealed class AdminCertificatesController : AdminControllerBase
{
    private readonly IAdminCertificateService _service;
    private readonly IValidator<CertificateFilterRequest> _filterValidator;
    private readonly IValidator<CertificateCreateRequest> _createValidator;
    private readonly IValidator<CertificateUpdateRequest> _updateValidator;

    public AdminCertificatesController(
        IAdminCertificateService service,
        IValidator<CertificateFilterRequest> filterValidator,
        IValidator<CertificateCreateRequest> createValidator,
        IValidator<CertificateUpdateRequest> updateValidator)
    {
        _service = service;
        _filterValidator = filterValidator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CertificateDto>>> GetAll(
        [FromQuery] CertificateFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var validation = await _filterValidator.ValidateAsync(filter, cancellationToken);
        if (!validation.IsValid) throw new RequestValidationException(validation.ToValidationDictionary());
        return Ok(await _service.GetAllAsync(filter, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<CertificateDto>> Create(
        [FromBody] CertificateCreateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new RequestValidationException(validation.ToValidationDictionary());
        var result = await _service.CreateAsync(request, GetCurrentUserId(), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CertificateDto>> Update(
        int id,
        [FromBody] CertificateUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new RequestValidationException(validation.ToValidationDictionary());
        return Ok(await _service.UpdateAsync(id, request, GetCurrentUserId(), cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<OperationResult>> Delete(int id, CancellationToken cancellationToken) =>
        Ok(await _service.DeleteAsync(id, GetCurrentUserId(), cancellationToken));

    [HttpPatch("{id:int}/toggle-active")]
    public async Task<ActionResult<CertificateDto>> ToggleActive(int id, CancellationToken cancellationToken) =>
        Ok(await _service.ToggleActiveAsync(id, GetCurrentUserId(), cancellationToken));

    [HttpPost("{id:int}/image")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FileUrlResponse>> UploadImage(
        int id,
        IFormFile file,
        CancellationToken cancellationToken) =>
        Ok(await _service.UploadImageAsync(id, file, GetCurrentUserId(), cancellationToken));
}

