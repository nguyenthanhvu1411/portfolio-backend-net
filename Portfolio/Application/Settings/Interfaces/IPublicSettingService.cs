using Portfolio.Application.Settings.DTOs;

namespace Portfolio.Application.Settings.Interfaces;

public interface IPublicSettingService
{
    Task<PublicSettingDto> GetAsync(
        CancellationToken cancellationToken = default);
}

