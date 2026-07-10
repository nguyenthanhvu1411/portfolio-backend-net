using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Common.Extensions;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Common.Security;
using Portfolio.Application.Settings.DTOs;
using Portfolio.Application.Settings.Interfaces;
using Portfolio.Common.Exceptions;

namespace Portfolio.Controllers.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminOnly)]
[Route("api/v1/admin/settings")]
[Produces("application/json")]
public sealed class AdminSettingsController : AdminControllerBase
{
    private readonly IAdminSettingService _service;
    private readonly IValidator<SettingUpdateRequest> _validator;

    public AdminSettingsController(
        IAdminSettingService service,
        IValidator<SettingUpdateRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    [HttpGet]
    public async Task<ActionResult<SettingDto>> Get(CancellationToken cancellationToken) =>
        Ok(await _service.GetAsync(cancellationToken));

    [HttpPut]
    public async Task<ActionResult<SettingDto>> Update(
        [FromBody] SettingUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new RequestValidationException(validation.ToValidationDictionary());
        }

        return Ok(await _service.UpdateAsync(
            request, GetCurrentUserId(), cancellationToken));
    }

    [HttpPost("logo")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FileUrlResponse>> UploadLogo(
        IFormFile file,
        CancellationToken cancellationToken) =>
        Ok(await _service.UploadLogoAsync(file, GetCurrentUserId(), cancellationToken));

    [HttpPost("favicon")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FileUrlResponse>> UploadFavicon(
        IFormFile file,
        CancellationToken cancellationToken) =>
        Ok(await _service.UploadFaviconAsync(file, GetCurrentUserId(), cancellationToken));
}

