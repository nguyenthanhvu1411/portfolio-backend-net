using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Blogs.DTOs;
using Portfolio.Application.Blogs.Interfaces;

namespace Portfolio.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/v1/public/blog-categories")]
[Produces("application/json")]
public sealed class PublicBlogCategoriesController
    : ControllerBase
{
    private readonly IPublicBlogService _service;

    public PublicBlogCategoriesController(
        IPublicBlogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<
        ActionResult<
            IReadOnlyList<PublicBlogCategoryDto>>>
        GetAll(
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetCategoriesAsync(
                cancellationToken));
    }
}

