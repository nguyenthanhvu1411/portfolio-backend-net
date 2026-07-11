using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Portfolio.Application.Profiles.Interfaces;
using Portfolio.Application.Projects.Interfaces;
using Portfolio.Infrastructure.Storage;

namespace Portfolio.Infrastructure.Services.Projects;

public static class ProjectServiceExtensions
{
    public static IServiceCollection AddAdminProjectModule(
        this IServiceCollection services)
    {
        services.AddScoped<IAdminProjectService, AdminProjectService>();

        return services;
    }
}
