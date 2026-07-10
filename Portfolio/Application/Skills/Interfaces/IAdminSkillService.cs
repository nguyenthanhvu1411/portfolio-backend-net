using Portfolio.Application.Common.Models;
using Portfolio.Application.Skills.DTOs;

namespace Portfolio.Application.Skills.Interfaces;

public interface IAdminSkillService
{
    Task<PagedResult<SkillDto>> GetPagedAsync(
        SkillFilterRequest filter,
        CancellationToken cancellationToken = default);

    Task<SkillDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<SkillDto> CreateAsync(
        SkillCreateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<SkillDto> UpdateAsync(
        int id,
        SkillUpdateRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<OperationResult> DeleteAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<SkillDto> ToggleActiveAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<SkillDto> ToggleFeaturedAsync(
        int id,
        int currentUserId,
        CancellationToken cancellationToken = default);

    Task<OperationResult> ReorderAsync(
        SkillReorderRequest request,
        int currentUserId,
        CancellationToken cancellationToken = default);
}
