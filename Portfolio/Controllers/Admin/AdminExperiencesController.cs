using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Common.Extensions;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Common.Security;
using Portfolio.Application.Experiences.DTOs;
using Portfolio.Application.Experiences.Interfaces;
using Portfolio.Common.Exceptions;

namespace Portfolio.Controllers.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminOnly)]
[Route("api/v1/admin/experiences")]
[Produces("application/json")]
public sealed class AdminExperiencesController : AdminControllerBase
{
    private readonly IAdminExperienceService _service;
    private readonly IValidator<ExperienceFilterRequest> _filterValidator;
    private readonly IValidator<ExperienceCreateRequest> _createValidator;
    private readonly IValidator<ExperienceUpdateRequest> _updateValidator;

    public AdminExperiencesController(
        IAdminExperienceService service,
        IValidator<ExperienceFilterRequest> filterValidator,
        IValidator<ExperienceCreateRequest> createValidator,
        IValidator<ExperienceUpdateRequest> updateValidator)
    {
        _service = service;
        _filterValidator = filterValidator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExperienceDto>>> GetAll(
        [FromQuery] ExperienceFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var validation = await _filterValidator.ValidateAsync(filter, cancellationToken);
        if (!validation.IsValid) throw new RequestValidationException(validation.ToValidationDictionary());
        return Ok(await _service.GetAllAsync(filter, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ExperienceDto>> Create(
        [FromBody] ExperienceCreateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new RequestValidationException(validation.ToValidationDictionary());
        var result = await _service.CreateAsync(request, GetCurrentUserId(), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ExperienceDto>> Update(
        int id,
        [FromBody] ExperienceUpdateRequest request,
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
    public async Task<ActionResult<ExperienceDto>> ToggleActive(int id, CancellationToken cancellationToken) =>
        Ok(await _service.ToggleActiveAsync(id, GetCurrentUserId(), cancellationToken));
}
