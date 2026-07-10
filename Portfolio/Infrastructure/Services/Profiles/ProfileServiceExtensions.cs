using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Profiles.Interfaces;
using Portfolio.Infrastructure.Storage;

namespace Portfolio.Infrastructure.Services.Profiles;

public static class ProfileServiceExtensions
{
    public static IServiceCollection AddAdminProfileModule(
        this IServiceCollection services)
    {
        services.AddScoped<IAdminProfileService, AdminProfileService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
