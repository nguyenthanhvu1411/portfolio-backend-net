using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Common.Extensions;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Common.Security;
using Portfolio.Application.Education.DTOs;
using Portfolio.Application.Education.Interfaces;
using Portfolio.Common.Exceptions;

namespace Portfolio.Controllers.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminOnly)]
[Route("api/v1/admin/education")]
[Produces("application/json")]
public sealed class AdminEducationController : AdminControllerBase
{
    private readonly IAdminEducationService _service;
    private readonly IValidator<EducationCreateRequest> _createValidator;
    private readonly IValidator<EducationUpdateRequest> _updateValidator;

    public AdminEducationController(
        IAdminEducationService service,
        IValidator<EducationCreateRequest> createValidator,
        IValidator<EducationUpdateRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EducationDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await _service.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<EducationDto>> Create(
        [FromBody] EducationCreateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new RequestValidationException(validation.ToValidationDictionary());
        var result = await _service.CreateAsync(request, GetCurrentUserId(), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EducationDto>> Update(
        int id,
        [FromBody] EducationUpdateRequest request,
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
    public async Task<ActionResult<EducationDto>> ToggleActive(int id, CancellationToken cancellationToken) =>
        Ok(await _service.ToggleActiveAsync(id, GetCurrentUserId(), cancellationToken));
}
