using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Blogs.Interfaces;
using Portfolio.Application.Contacts.Interfaces;
using Portfolio.Application.Profiles.Interfaces;
using Portfolio.Application.Projects.Interfaces;
using Portfolio.Application.Resume.Interfaces;
using Portfolio.Application.Common.Security;
using Portfolio.Application.Settings.Interfaces;
using Portfolio.Application.Skills.Interfaces;
using Portfolio.Infrastructure.Services.Blogs;
using Portfolio.Infrastructure.Services.Contacts;
using Portfolio.Infrastructure.Services.Profiles;
using Portfolio.Infrastructure.Services.Projects;
using Portfolio.Infrastructure.Services.Resumes;
using Portfolio.Infrastructure.Services.Settings;
using Portfolio.Infrastructure.Services.Skills;

namespace Portfolio.Infrastructure.Services;

public static class PublicServiceExtensions
{
    public static IServiceCollection AddPublicPortfolioModule(
        this IServiceCollection services)
    {
        services.AddScoped<
            IPublicProfileService,
            PublicProfileService>();

        services.AddScoped<
            IPublicSettingService,
            PublicSettingService>();

        services.AddScoped<
            IPublicSkillService,
            PublicSkillService>();

        services.AddScoped<
            IPublicProjectService,
            PublicProjectService>();

        services.AddScoped<
            IPublicResumeService,
            PublicResumeService>();

        services.AddScoped<
            IPublicBlogService,
            PublicBlogService>();

        services.AddScoped<
            IPublicContactService,
            PublicContactService>();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;

            options.AddPolicy(
                PublicRateLimitPolicies.ContactSubmit,
                httpContext =>
                {
                    var clientKey =
                        httpContext.Connection
                            .RemoteIpAddress?
                            .ToString()
                        ?? "unknown";

                    return RateLimitPartition
                        .GetFixedWindowLimiter(
                            clientKey,
                            _ =>
                                new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = 5,
                                    Window =
                                        TimeSpan.FromMinutes(10),
                                    QueueLimit = 0,
                                    QueueProcessingOrder =
                                        QueueProcessingOrder
                                            .OldestFirst,
                                    AutoReplenishment = true
                                });
                });

            options.AddPolicy(
                PublicRateLimitPolicies.BlogView,
                httpContext =>
                {
                    var clientKey =
                        httpContext.Connection
                            .RemoteIpAddress?
                            .ToString()
                        ?? "unknown";

                    return RateLimitPartition
                        .GetFixedWindowLimiter(
                            clientKey,
                            _ =>
                                new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = 30,
                                    Window =
                                        TimeSpan.FromMinutes(1),
                                    QueueLimit = 0,
                                    QueueProcessingOrder =
                                        QueueProcessingOrder
                                            .OldestFirst,
                                    AutoReplenishment = true
                                });
                });
        });

        return services;
    }
}

