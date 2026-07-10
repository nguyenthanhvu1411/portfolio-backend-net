using Portfolio.Application.Skills.DTOs;

namespace Portfolio.Application.Skills.Interfaces;

public interface IPublicSkillService
{
    Task<IReadOnlyList<PublicSkillDto>> GetFeaturedAsync(
        int limit,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PublicSkillDto>> GetAllAsync(
        PublicSkillFilterRequest filter,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PublicSkillCategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken = default);
}

