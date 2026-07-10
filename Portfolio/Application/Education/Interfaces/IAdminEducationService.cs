using Portfolio.Application.Common.Models;
using Portfolio.Application.Education.DTOs;

namespace Portfolio.Application.Education.Interfaces;

public interface IAdminEducationService
{
    Task<IReadOnlyList<EducationDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EducationDto> CreateAsync(EducationCreateRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<EducationDto> UpdateAsync(int id, EducationUpdateRequest request, int currentUserId, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
    Task<EducationDto> ToggleActiveAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
}
