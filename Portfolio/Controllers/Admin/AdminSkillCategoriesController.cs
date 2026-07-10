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
[Route("api/v1/admin/skill-categories")]
[Produces("application/json")]
public sealed class AdminSkillCategoriesController : AdminControllerBase
{
    private readonly IAdminSkillCategoryService _service;
    private readonly IValidator<SkillCategoryCreateRequest> _createValidator;
    private readonly IValidator<SkillCategoryUpdateRequest> _updateValidator;

    public AdminSkillCategoriesController(
        IAdminSkillCategoryService service,
        IValidator<SkillCategoryCreateRequest> createValidator,
        IValidator<SkillCategoryUpdateRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<SkillCategoryDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SkillCategoryDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAllAsync(cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(SkillCategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SkillCategoryDto>> Create(
        [FromBody] SkillCategoryCreateRequest request,
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

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(SkillCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SkillCategoryDto>> Update(
        int id,
        [FromBody] SkillCategoryUpdateRequest request,
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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<OperationResult>> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.DeleteAsync(
            id,
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
            Detail = "Vui lÃ²ng kiá»ƒm tra láº¡i thÃ´ng tin nhÃ³m ká»¹ nÄƒng.",
            Type = "https://httpstatuses.com/400",
            Instance = HttpContext.Request.Path
        };

        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
        return problem;
    }
}

