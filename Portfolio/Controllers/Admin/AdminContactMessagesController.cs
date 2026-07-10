using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Common.Extensions;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Common.Security;
using Portfolio.Application.Contacts.DTOs;
using Portfolio.Application.Contacts.Interfaces;

namespace Portfolio.Controllers.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminOnly)]
[Route("api/v1/admin/contact-messages")]
[Produces("application/json")]
public sealed class AdminContactMessagesController : AdminControllerBase
{
    private readonly IAdminContactMessageService _service;
    private readonly IValidator<ContactMessageFilterRequest>
        _filterValidator;

    public AdminContactMessagesController(
        IAdminContactMessageService service,
        IValidator<ContactMessageFilterRequest> filterValidator)
    {
        _service = service;
        _filterValidator = filterValidator;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<ContactMessageDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    public async Task<
        ActionResult<PagedResult<ContactMessageDto>>> GetPaged(
        [FromQuery] ContactMessageFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var validation =
            await _filterValidator.ValidateAsync(
                filter,
                cancellationToken);

        if (!validation.IsValid)
        {
            return ValidationProblem(
                CreateValidationProblem(
                    validation.ToValidationDictionary()));
        }

        return Ok(
            await _service.GetPagedAsync(
                filter,
                cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(ContactMessageDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactMessageDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        // Tin nháº¯n New Ä‘Æ°á»£c tá»± Ä‘á»™ng chuyá»ƒn sang Read khi Admin má»Ÿ chi tiáº¿t.
        return Ok(
            await _service.GetByIdAsync(
                id,
                GetCurrentUserId(),
                autoMarkAsRead: true,
                cancellationToken: cancellationToken));
    }

    [HttpPatch("{id:int}/mark-as-read")]
    [ProducesResponseType(
        typeof(ContactMessageDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactMessageDto>> MarkAsRead(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.MarkAsReadAsync(
                id,
                GetCurrentUserId(),
                cancellationToken));
    }

    [HttpPatch("{id:int}/mark-as-replied")]
    [ProducesResponseType(
        typeof(ContactMessageDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContactMessageDto>> MarkAsReplied(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.MarkAsRepliedAsync(
                id,
                GetCurrentUserId(),
                cancellationToken));
    }

    [HttpPatch("{id:int}/archive")]
    [ProducesResponseType(
        typeof(ContactMessageDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactMessageDto>> Archive(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.ArchiveAsync(
                id,
                GetCurrentUserId(),
                cancellationToken));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(
        typeof(OperationResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OperationResult>> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.DeleteAsync(
                id,
                GetCurrentUserId(),
                cancellationToken));
    }

    private ValidationProblemDetails CreateValidationProblem(
        IDictionary<string, string[]> errors)
    {
        var problem =
            new ValidationProblemDetails(errors)
            {
                Status =
                    StatusCodes.Status400BadRequest,
                Title = "Dá»¯ liá»‡u khÃ´ng há»£p lá»‡",
                Detail =
                    "Vui lÃ²ng kiá»ƒm tra láº¡i bá»™ lá»c tin nháº¯n liÃªn há»‡.",
                Type = "https://httpstatuses.com/400",
                Instance = HttpContext.Request.Path
            };

        problem.Extensions["traceId"] =
            HttpContext.TraceIdentifier;

        return problem;
    }
}

