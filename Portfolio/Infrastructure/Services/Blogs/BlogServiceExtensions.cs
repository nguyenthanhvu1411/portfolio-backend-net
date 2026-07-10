using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Blogs.Interfaces;

namespace Portfolio.Infrastructure.Services.Blogs;

public static class BlogServiceExtensions
{
    public static IServiceCollection AddAdminBlogModule(this IServiceCollection services)
    {
        services.AddScoped<IAdminBlogCategoryService, AdminBlogCategoryService>();
        services.AddScoped<IAdminBlogService, AdminBlogService>();
        return services;
    }
}
