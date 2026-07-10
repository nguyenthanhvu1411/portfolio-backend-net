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
[Route("api/v1/admin/blog-categories")]
[Produces("application/json")]
public sealed class AdminBlogCategoriesController : AdminControllerBase
{
    private readonly IAdminBlogCategoryService _service;
    private readonly IValidator<BlogCategoryCreateRequest> _createValidator;
    private readonly IValidator<BlogCategoryUpdateRequest> _updateValidator;

    public AdminBlogCategoriesController(
        IAdminBlogCategoryService service,
        IValidator<BlogCategoryCreateRequest> createValidator,
        IValidator<BlogCategoryUpdateRequest> updateValidator)
    {
        _service = service;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BlogCategoryDto>>> GetAll(
        CancellationToken cancellationToken) =>
        Ok(await _service.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<BlogCategoryDto>> Create(
        [FromBody] BlogCategoryCreateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new RequestValidationException(validation.ToValidationDictionary());
        var result = await _service.CreateAsync(request, GetCurrentUserId(), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BlogCategoryDto>> Update(
        int id,
        [FromBody] BlogCategoryUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) throw new RequestValidationException(validation.ToValidationDictionary());
        return Ok(await _service.UpdateAsync(id, request, GetCurrentUserId(), cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<OperationResult>> Delete(
        int id,
        CancellationToken cancellationToken) =>
        Ok(await _service.DeleteAsync(id, GetCurrentUserId(), cancellationToken));
}
