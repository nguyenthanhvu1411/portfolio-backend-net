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

        // Dùng chung LocalFileStorageService với module Profile.
        // TryAdd tránh đăng ký trùng nếu AddAdminProfileModule đã được gọi.
        services.TryAddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
