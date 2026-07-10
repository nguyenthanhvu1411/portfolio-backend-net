using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Skills.DTOs;
using Portfolio.Application.Skills.Interfaces;

namespace Portfolio.Controllers.Public;

[ApiController]
[AllowAnonymous]
[Route("api/v1/public/skill-categories")]
[Produces("application/json")]
public sealed class PublicSkillCategoriesController
    : ControllerBase
{
    private readonly IPublicSkillService _service;

    public PublicSkillCategoriesController(
        IPublicSkillService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<
        ActionResult<
            IReadOnlyList<PublicSkillCategoryDto>>>
        GetAll(
            CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetCategoriesAsync(
                cancellationToken));
    }
}

