using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Common.Extensions;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Common.Security;
using Portfolio.Application.Projects.DTOs;
using Portfolio.Application.Projects.Interfaces;

namespace Portfolio.Controllers.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminOnly)]
[Route("api/v1/admin/projects")]
[Produces("application/json")]
public sealed class AdminProjectsController : AdminControllerBase
{
    private const long UploadRequestLimit = 12 * 1024 * 1024;

    private readonly IAdminProjectService _service;
    private readonly IValidator<ProjectFilterRequest> _filterValidator;
    private readonly IValidator<ProjectCreateRequest> _createValidator;
    private readonly IValidator<ProjectUpdateRequest> _updateValidator;
    private readonly IValidator<ProjectImageUploadRequest> _imageUploadValidator;

    public AdminProjectsController(
        IAdminProjectService service,
        IValidator<ProjectFilterRequest> filterValidator,
        IValidator<ProjectCreateRequest> createValidator,
        IValidator<ProjectUpdateRequest> updateValidator,
        IValidator<ProjectImageUploadRequest> imageUploadValidator)
    {
        _service = service;
        _filterValidator = filterValidator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _imageUploadValidator = imageUploadValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ProjectDto>>> GetPaged(
        [FromQuery] ProjectFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var validation = await _filterValidator.ValidateAsync(filter, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationProblem(CreateValidationProblem(
                validation.ToValidationDictionary(),
                "Vui lÃ²ng kiá»ƒm tra bá»™ lá»c dá»± Ã¡n."));
        }

        return Ok(await _service.GetPagedAsync(filter, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetByIdAsync(id, cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProjectDto>> Create(
        [FromBody] ProjectCreateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationProblem(CreateValidationProblem(
                validation.ToValidationDictionary(),
                "Vui lÃ²ng kiá»ƒm tra thÃ´ng tin dá»± Ã¡n."));
        }

        var result = await _service.CreateAsync(
            request,
            GetCurrentUserId(),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProjectDto>> Update(
        int id,
        [FromBody] ProjectUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationProblem(CreateValidationProblem(
                validation.ToValidationDictionary(),
                "Vui lÃ²ng kiá»ƒm tra thÃ´ng tin dá»± Ã¡n."));
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
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> ToggleActive(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.ToggleActiveAsync(
            id,
            GetCurrentUserId(),
            cancellationToken));
    }

    [HttpPatch("{id:int}/toggle-featured")]
    [ProducesResponseType(typeof(ProjectDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDto>> ToggleFeatured(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.ToggleFeaturedAsync(
            id,
            GetCurrentUserId(),
            cancellationToken));
    }

    [HttpPost("{id:int}/thumbnail")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(UploadRequestLimit)]
    [ProducesResponseType(typeof(ProjectFileUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectFileUploadResponse>> UploadThumbnail(
        int id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.UploadThumbnailAsync(
            id,
            file,
            GetCurrentUserId(),
            cancellationToken));
    }

    [HttpGet("{projectId:int}/images")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ProjectImageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<ProjectImageDto>>> GetImages(
        int projectId,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetImagesAsync(projectId, cancellationToken));
    }

    [HttpPost("{projectId:int}/images")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(UploadRequestLimit)]
    [ProducesResponseType(typeof(ProjectImageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectImageDto>> UploadImage(
        int projectId,
        [FromForm] ProjectImageUploadRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _imageUploadValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationProblem(CreateValidationProblem(
                validation.ToValidationDictionary(),
                "Vui lÃ²ng kiá»ƒm tra áº£nh dá»± Ã¡n."));
        }

        var result = await _service.UploadImageAsync(
            projectId,
            request,
            GetCurrentUserId(),
            cancellationToken);

        return CreatedAtAction(nameof(GetImages), new { projectId }, result);
    }

    private ValidationProblemDetails CreateValidationProblem(
        IDictionary<string, string[]> errors,
        string detail)
    {
        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Dá»¯ liá»‡u khÃ´ng há»£p lá»‡",
            Detail = detail,
            Type = "https://httpstatuses.com/400",
            Instance = HttpContext.Request.Path
        };

        problem.Extensions["traceId"] = HttpContext.TraceIdentifier;
        return problem;
    }
}

