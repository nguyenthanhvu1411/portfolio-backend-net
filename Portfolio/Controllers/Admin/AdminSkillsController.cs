using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Common.Extensions;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Common.Security;
using Portfolio.Application.Skills.DTOs;
using Portfolio.Application.Skills.Interfaces;

namespace Portfolio.Controllers.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminOnly)]
[Route("api/v1/admin/skills")]
[Produces("application/json")]
public sealed class AdminSkillsController : AdminControllerBase
{
    private readonly IAdminSkillService _service;
    private readonly IValidator<SkillFilterRequest> _filterValidator;
    private readonly IValidator<SkillCreateRequest> _createValidator;
    private readonly IValidator<SkillUpdateRequest> _updateValidator;
    private readonly IValidator<SkillReorderRequest> _reorderValidator;

    public AdminSkillsController(
        IAdminSkillService service,
        IValidator<SkillFilterRequest> filterValidator,
        IValidator<SkillCreateRequest> createValidator,
        IValidator<SkillUpdateRequest> updateValidator,
        IValidator<SkillReorderRequest> reorderValidator)
    {
        _service = service;
        _filterValidator = filterValidator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _reorderValidator = reorderValidator;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<SkillDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<SkillDto>>> GetPaged(
        [FromQuery] SkillFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var validationResult = await _filterValidator.ValidateAsync(
            filter,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return ValidationProblem(
                CreateValidationProblem(
                    validationResult.ToValidationDictionary()));
        }

        return Ok(await _service.GetPagedAsync(filter, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SkillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SkillDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(SkillDto), StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SkillDto>> Create(
        [FromBody] SkillCreateRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createValidator.ValidateAsync(
            request,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return ValidationProblem(
                CreateValidationProblem(
                    validationResult.ToValidationDictionary()));
        }

        var result = await _service.CreateAsync(
            request,
            GetCurrentUserId(),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(SkillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SkillDto>> Update(
        int id,
        [FromBody] SkillUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _updateValidator.ValidateAsync(
            request,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return ValidationProblem(
                CreateValidationProblem(
                    validationResult.ToValidationDictionary()));
        }

        return Ok(await _service.UpdateAsync(
            id,
            request,
            GetCurrentUserId(),
            cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OperationResult>> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.DeleteAsync(
            id,
            GetCurrentUserId(),
            cancellationToken));
    }

    [HttpPatch("{id:int}/toggle-active")]
    [ProducesResponseType(typeof(SkillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SkillDto>> ToggleActive(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.ToggleActiveAsync(
            id,
            GetCurrentUserId(),
            cancellationToken));
    }

    [HttpPatch("{id:int}/toggle-featured")]
    [ProducesResponseType(typeof(SkillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SkillDto>> ToggleFeatured(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.ToggleFeaturedAsync(
            id,
            GetCurrentUserId(),
            cancellationToken));
    }

    [HttpPut("reorder")]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OperationResult>> Reorder(
        [FromBody] SkillReorderRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _reorderValidator.ValidateAsync(
            request,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return ValidationProblem(
                CreateValidationProblem(
                    validationResult.ToValidationDictionary()));
        }

        return Ok(await _service.ReorderAsync(
            request,
            GetCurrentUserId(),
            cancellationToken));
    }

    private ValidationProblemDetails CreateValidationProblem(
        IDictionary<string, string[]> errors)
    {
        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Dá»¯ liá»‡u khÃ´ng há»£p lá»‡",
            Detail = "Vui lÃ²ng kiá»ƒm tra láº¡i thÃ´ng tin ká»¹ nÄƒng.",
            Type = "https://httpstatuses.com/400",
            Instance = HttpContext.Request.Path
        };

        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
        return problem;
    }
}

