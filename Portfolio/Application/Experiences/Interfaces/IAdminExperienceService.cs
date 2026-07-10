using Portfolio.Application.Common.Models;
using Portfolio.Application.Experiences.DTOs;

namespace Portfolio.Application.Experiences.Interfaces;

public interface IAdminExperienceService
{
    Task<IReadOnlyList<ExperienceDto>> GetAllAsync(ExperienceFilterRequest filter, CancellationToken cancellationToken = default);
    Task<ExperienceDto> CreateAsync(ExperienceCreateRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<ExperienceDto> UpdateAsync(int id, ExperienceUpdateRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
    Task<ExperienceDto> ToggleActiveAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
}
