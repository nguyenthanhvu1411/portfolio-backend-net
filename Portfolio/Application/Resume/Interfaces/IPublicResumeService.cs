using Portfolio.Application.Resume.DTOs;

namespace Portfolio.Application.Resume.Interfaces;

public interface IPublicResumeService
{
    Task<IReadOnlyList<PublicExperienceDto>> GetExperiencesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PublicEducationDto>> GetEducationAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PublicCertificateDto>> GetCertificatesAsync(
        PublicCertificateFilterRequest filter,
        CancellationToken cancellationToken = default);
}
