using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Users.Interfaces;

namespace Portfolio.Infrastructure.Services.Users;

public static class UserServiceExtensions
{
    public static IServiceCollection AddAdminUserModule(this IServiceCollection services)
    {
        services.AddScoped<IAdminUserService, AdminUserService>();
        return services;
    }
}
