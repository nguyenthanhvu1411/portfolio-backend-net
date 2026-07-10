using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Certificates.Interfaces;

namespace Portfolio.Infrastructure.Services.Certificates;

public static class CertificateServiceExtensions
{
    public static IServiceCollection AddAdminCertificateModule(this IServiceCollection services)
    {
        services.AddScoped<IAdminCertificateService, AdminCertificateService>();
        return services;
    }
}
