using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Experiences.Interfaces;

namespace Portfolio.Infrastructure.Services.Experiences;

public static class ExperienceServiceExtensions
{
    public static IServiceCollection AddAdminExperienceModule(this IServiceCollection services)
    {
        services.AddScoped<IAdminExperienceService, AdminExperienceService>();
        return services;
    }
}
