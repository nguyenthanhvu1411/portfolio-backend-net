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
[Route("api/v1/admin/project-images")]
[Produces("application/json")]
public sealed class AdminProjectImagesController : AdminControllerBase
{
    private readonly IAdminProjectService _service;
    private readonly IValidator<ProjectImageUpdateRequest> _updateValidator;

    public AdminProjectImagesController(
        IAdminProjectService service,
        IValidator<ProjectImageUpdateRequest> updateValidator)
    {
        _service = service;
        _updateValidator = updateValidator;
    }

    [HttpPut("{imageId:int}")]
    [ProducesResponseType(typeof(ProjectImageDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectImageDto>> Update(
        int imageId,
        [FromBody] ProjectImageUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(
                validation.ToValidationDictionary())
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Dá»¯ liá»‡u khÃ´ng há»£p lá»‡",
                Detail = "Vui lÃ²ng kiá»ƒm tra caption vÃ  thá»© tá»± áº£nh.",
                Type = "https://httpstatuses.com/400",
                Instance = HttpContext.Request.Path
            });
        }

        return Ok(await _service.UpdateImageAsync(
            imageId,
            request,
            GetCurrentUserId(),
            cancellationToken));
    }

    [HttpDelete("{imageId:int}")]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OperationResult>> Delete(
        int imageId,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.DeleteImageAsync(
            imageId,
            GetCurrentUserId(),
            cancellationToken));
    }

    [HttpPatch("{imageId:int}/set-thumbnail")]
    [ProducesResponseType(typeof(OperationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OperationResult>> SetThumbnail(
        int imageId,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.SetThumbnailAsync(
            imageId,
            GetCurrentUserId(),
            cancellationToken));
    }
}

