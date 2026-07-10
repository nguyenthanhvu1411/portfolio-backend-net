using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Contacts.Interfaces;

namespace Portfolio.Infrastructure.Services.Contacts;

public static class ContactServiceExtensions
{
    public static IServiceCollection AddAdminContactModule(
        this IServiceCollection services)
    {
        services.AddScoped<
            IAdminContactMessageService,
            AdminContactMessageService>();

        return services;
    }
}
