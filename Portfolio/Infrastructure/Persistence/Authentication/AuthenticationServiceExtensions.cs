using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Portfolio.Application.Auth.Interfaces;

namespace Portfolio.Infrastructure.Authentication;

public static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddAdminAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(
            JwtSettings.SectionName);

        if (!jwtSection.Exists())
        {
            throw new InvalidOperationException(
                $"Không tìm thấy cấu hình {JwtSettings.SectionName}.");
        }

        var jwtSettings = jwtSection.Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                $"Không thể đọc cấu hình {JwtSettings.SectionName}.");

        ValidateJwtSettings(jwtSettings);

        services
            .AddOptions<JwtSettings>()
            .Bind(jwtSection)
            .Validate(
                settings =>
                    !string.IsNullOrWhiteSpace(settings.Secret) &&
                    Encoding.UTF8.GetByteCount(settings.Secret) >= 32,
                "JwtSettings:Secret phải có tối thiểu 32 byte.")
            .Validate(
                settings =>
                    !string.IsNullOrWhiteSpace(settings.Issuer),
                "JwtSettings:Issuer không được để trống.")
            .Validate(
                settings =>
                    !string.IsNullOrWhiteSpace(settings.Audience),
                "JwtSettings:Audience không được để trống.")
            .Validate(
                settings => settings.ExpiryMinutes > 0,
                "JwtSettings:ExpiryMinutes phải lớn hơn 0.")
            .Validate(
                settings => settings.RefreshTokenExpiryDays > 0,
                "JwtSettings:RefreshTokenExpiryDays phải lớn hơn 0.")
            .ValidateOnStart();

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Secret));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.SaveToken = true;
                options.RequireHttpsMetadata = true;

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,

                        ValidateLifetime = true,
                        RequireExpirationTime = true,

                        ClockSkew = TimeSpan.Zero,

                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                "AdminOnly",
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("SuperAdmin", "Admin");
                });

            options.AddPolicy(
                "SuperAdminOnly",
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireRole("SuperAdmin");
                });
        });

        services.AddHttpContextAccessor();

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddScoped<IAuthService, AuthService>();
        // services.AddScoped<ICurrentUserService, CurrentUserService>(); // Giữ lại nếu có

        return services;
    }

    private static void ValidateJwtSettings(
        JwtSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Secret))
        {
            throw new InvalidOperationException(
                "JwtSettings:Secret không được để trống.");
        }

        if (Encoding.UTF8.GetByteCount(settings.Secret) < 32)
        {
            throw new InvalidOperationException(
                "JwtSettings:Secret phải có tối thiểu 32 byte.");
        }

        if (string.IsNullOrWhiteSpace(settings.Issuer))
        {
            throw new InvalidOperationException(
                "JwtSettings:Issuer không được để trống.");
        }

        if (string.IsNullOrWhiteSpace(settings.Audience))
        {
            throw new InvalidOperationException(
                "JwtSettings:Audience không được để trống.");
        }

        if (settings.ExpiryMinutes <= 0)
        {
            throw new InvalidOperationException(
                "JwtSettings:ExpiryMinutes phải lớn hơn 0.");
        }

        if (settings.RefreshTokenExpiryDays <= 0)
        {
            throw new InvalidOperationException(
                "JwtSettings:RefreshTokenExpiryDays phải lớn hơn 0.");
        }
    }
}