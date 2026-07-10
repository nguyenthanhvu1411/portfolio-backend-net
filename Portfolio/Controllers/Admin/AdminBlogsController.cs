using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Blogs.DTOs;
using Portfolio.Application.Blogs.Interfaces;
using Portfolio.Application.Common.Extensions;
using Portfolio.Application.Common.Models;
using Portfolio.Application.Common.Security;
using Portfolio.Common.Exceptions;

namespace Portfolio.Controllers.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminOnly)]
[Route("api/v1/admin/blogs")]
[Produces("application/json")]
public sealed class AdminBlogsController : AdminControllerBase
{
    private readonly IAdminBlogService _service;
    private readonly IValidator<BlogFilterRequest> _filterValidator;
    private readonly IValidator<BlogCreateRequest> _createValidator;
    private readonly IValidator<BlogUpdateRequest> _updateValidator;

    public AdminBlogsController(
        IAdminBlogService service,
        IValidator<BlogFilterRequest> filterValidator,
        IValidator<BlogCreateRequest> createValidator,
        IValidator<BlogUpdateRequest> updateValidator)
    {
        _service = service;
        _filterValidator = filterValidator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<BlogDto>>> GetPaged(
        [FromQuery] BlogFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var validation = await _filterValidator.ValidateAsync(filter, cancellationToken);
        if (!validation.IsValid) throw new RequestValidationException(validation.ToValidationDictionary());
        return Ok(await _service.GetPagedAsync(filter, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BlogDetailDto>> GetById(
        int id,
        CancellationToken cancellationToken) =>
        Ok(await _service.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<BlogDto>> Create(
        [FromBody] BlogCreateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new RequestValidationException(validation.ToValidationDictionary());
        var result = await _service.CreateAsync(request, GetCurrentUserId(), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BlogDto>> Update(
        int id,
        [FromBody] BlogUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new RequestValidationException(validation.ToValidationDictionary());
        return Ok(await _service.UpdateAsync(id, request, GetCurrentUserId(), cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<OperationResult>> Delete(int id, CancellationToken cancellationToken) =>
        Ok(await _service.DeleteAsync(id, GetCurrentUserId(), cancellationToken));

    [HttpPatch("{id:int}/publish")]
    public async Task<ActionResult<BlogDto>> Publish(int id, CancellationToken cancellationToken) =>
        Ok(await _service.PublishAsync(id, GetCurrentUserId(), cancellationToken));

    [HttpPatch("{id:int}/unpublish")]
    public async Task<ActionResult<BlogDto>> Unpublish(int id, CancellationToken cancellationToken) =>
        Ok(await _service.UnpublishAsync(id, GetCurrentUserId(), cancellationToken));

    [HttpPatch("{id:int}/toggle-featured")]
    public async Task<ActionResult<BlogDto>> ToggleFeatured(int id, CancellationToken cancellationToken) =>
        Ok(await _service.ToggleFeaturedAsync(id, GetCurrentUserId(), cancellationToken));

    [HttpPost("{id:int}/thumbnail")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FileUrlResponse>> UploadThumbnail(
        int id,
        IFormFile file,
        CancellationToken cancellationToken) =>
        Ok(await _service.UploadThumbnailAsync(id, file, GetCurrentUserId(), cancellationToken));
}

