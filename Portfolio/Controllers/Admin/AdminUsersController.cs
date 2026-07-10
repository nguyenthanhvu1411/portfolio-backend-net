using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Common.Extensions;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Common.Security;
using Portfolio.Application.Users.DTOs;
using Portfolio.Application.Users.Interfaces;
using Portfolio.Common.Exceptions;

namespace Portfolio.Controllers.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminOnly)]
[Route("api/v1/admin/users")]
[Produces("application/json")]
public sealed class AdminUsersController : AdminControllerBase
{
    private readonly IAdminUserService _service;
    private readonly IValidator<UserFilterRequest> _filterValidator;
    private readonly IValidator<UserCreateRequest> _createValidator;
    private readonly IValidator<UserUpdateRequest> _updateValidator;
    private readonly IValidator<ResetUserPasswordRequest> _resetValidator;

    public AdminUsersController(
        IAdminUserService service,
        IValidator<UserFilterRequest> filterValidator,
        IValidator<UserCreateRequest> createValidator,
        IValidator<UserUpdateRequest> updateValidator,
        IValidator<ResetUserPasswordRequest> resetValidator)
    {
        _service = service;
        _filterValidator = filterValidator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _resetValidator = resetValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(
        [FromQuery] UserFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var validation = await _filterValidator.ValidateAsync(filter, cancellationToken);
        if (!validation.IsValid)
        {
            throw new RequestValidationException(validation.ToValidationDictionary());
        }

        return Ok(await _service.GetAllAsync(filter, cancellationToken));
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(
        [FromBody] UserCreateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new RequestValidationException(validation.ToValidationDictionary());
        }

        var result = await _service.CreateAsync(
            request, GetCurrentUserId(), cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> Update(
        int id,
        [FromBody] UserUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new RequestValidationException(validation.ToValidationDictionary());
        }

        return Ok(await _service.UpdateAsync(
            id, request, GetCurrentUserId(), cancellationToken));
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPatch("{id:int}/lock")]
    public async Task<ActionResult<UserDto>> Lock(int id, CancellationToken cancellationToken)
    {
        return Ok(await _service.LockAsync(id, GetCurrentUserId(), cancellationToken));
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPatch("{id:int}/unlock")]
    public async Task<ActionResult<UserDto>> Unlock(int id, CancellationToken cancellationToken)
    {
        return Ok(await _service.UnlockAsync(id, GetCurrentUserId(), cancellationToken));
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPatch("{id:int}/reset-password")]
    public async Task<ActionResult<OperationResult>> ResetPassword(
        int id,
        [FromBody] ResetUserPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _resetValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new RequestValidationException(validation.ToValidationDictionary());
        }

        return Ok(await _service.ResetPasswordAsync(
            id, request, GetCurrentUserId(), cancellationToken));
    }
}
