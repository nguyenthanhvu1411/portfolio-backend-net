using Portfolio.Application.Common.Models;
using Portfolio.Application.Skills.DTOs;

namespace Portfolio.Application.Skills.Interfaces;

public interface IAdminSkillCategoryService
{
    Task<IReadOnlyList<SkillCategoryDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<SkillCategoryDto> CreateAsync(
        SkillCategoryCreateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<SkillCategoryDto> UpdateAsync(
        int id,
        SkillCategoryUpdateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<OperationResult> DeleteAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default);
}
