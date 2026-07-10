using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Skills.Interfaces;

namespace Portfolio.Infrastructure.Services.Skills;

public static class SkillServiceExtensions
{
    public static IServiceCollection AddAdminSkillModule(
        this IServiceCollection services)
    {
        services.AddScoped<
            IAdminSkillCategoryService,
            AdminSkillCategoryService>();

        services.AddScoped<
            IAdminSkillService,
            AdminSkillService>();

        return services;
    }
}
