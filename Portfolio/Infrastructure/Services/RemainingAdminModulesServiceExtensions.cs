using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Portfolio.Application.Profiles.Interfaces;
using Portfolio.Infrastructure.Services.Blogs;
using Portfolio.Infrastructure.Services.Certificates;
using Portfolio.Infrastructure.Services.Contacts;
using Portfolio.Infrastructure.Services.Education;
using Portfolio.Infrastructure.Services.Experiences;
using Portfolio.Infrastructure.Services.Profiles;
using Portfolio.Infrastructure.Services.Projects;
using Portfolio.Infrastructure.Services.Settings;
using Portfolio.Infrastructure.Services.Skills;
using Portfolio.Infrastructure.Services.Uploads;
using Portfolio.Infrastructure.Services.Users;
using Portfolio.Infrastructure.Storage;

namespace Portfolio.Infrastructure.Services;

public static class RemainingAdminModulesServiceExtensions
{
    public static IServiceCollection AddRemainingAdminModules(this IServiceCollection services)
    {
        services.AddAdminUserModule();
        services.AddAdminSettingModule();
        services.AddAdminUploadModule();
        services.AddAdminExperienceModule();
        services.AddAdminEducationModule();
        services.AddAdminCertificateModule();
        services.AddAdminBlogModule();
        services.AddAdminProjectModule();
        services.AddAdminContactModule();
        services.AddAdminSkillModule();
        services.AddAdminProfileModule();
        return services;
    }
}
