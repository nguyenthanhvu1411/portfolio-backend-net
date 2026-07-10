using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Settings.Interfaces;

namespace Portfolio.Infrastructure.Services.Settings;

public static class SettingServiceExtensions
{
    public static IServiceCollection AddAdminSettingModule(this IServiceCollection services)
    {
        services.AddScoped<IAdminSettingService, AdminSettingService>();
        return services;
    }
}
