using Portfolio.Application.Profiles.Models;
using Portfolio.Application.Profiles.DTOs;

namespace Portfolio.Application.Profiles.Interfaces;

public interface IPublicProfileService
{
    Task<PublicProfileDto> GetAsync(
        CancellationToken cancellationToken = default);

    Task<PublicCvResource> GetCvAsync(
        CancellationToken cancellationToken = default);
}

