using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Education.Interfaces;

namespace Portfolio.Infrastructure.Services.Education;

public static class EducationServiceExtensions
{
    public static IServiceCollection AddAdminEducationModule(this IServiceCollection services)
    {
        services.AddScoped<IAdminEducationService, AdminEducationService>();
        return services;
    }
}
