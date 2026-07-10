using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Uploads.Interfaces;

namespace Portfolio.Infrastructure.Services.Uploads;

public static class UploadServiceExtensions
{
    public static IServiceCollection AddAdminUploadModule(this IServiceCollection services)
    {
        services.AddScoped<IAdminUploadService, AdminUploadService>();
        return services;
    }
}
