using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Portfolio.Application.Auth.Validators;
using Portfolio.Application.Common.Security;
using Portfolio.Common.Exceptions;
using Portfolio.Domain.Entities;
using System.Threading.RateLimiting;
using Portfolio.Infrastructure.Authentication;
using Portfolio.Infrastructure.Persistence;
using Portfolio.Infrastructure.Persistence.Seed;
using Portfolio.Infrastructure.Services;
using Portfolio.Application.Common.Email;
using Portfolio.Infrastructure.Email;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problem = new ValidationProblemDetails(
                context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Dữ liệu không hợp lệ",
                Detail =
                    "Không thể đọc dữ liệu yêu cầu hoặc dữ liệu sai định dạng.",
                Type = "https://httpstatuses.com/400",
                Instance = context.HttpContext.Request.Path
            };

            problem.Extensions["traceId"] =
                context.HttpContext.TraceIdentifier;

            return new BadRequestObjectResult(problem);
        };
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Nhập access token JWT."
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection")
    ?? throw new InvalidOperationException(
        "Không tìm thấy ConnectionStrings:DefaultConnection.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options
        .UseSqlServer(connectionString)
        .UseSnakeCaseNamingConvention();
});

builder.Services.Configure<AdminSeedOptions>(
    builder.Configuration.GetSection(
        AdminSeedOptions.SectionName));

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>>();

builder.Services.AddScoped<DatabaseSeeder>();

builder.Services.AddValidatorsFromAssemblyContaining<
    AdminLoginRequestValidator>();

builder.Services.AddAdminAuthentication(
    builder.Configuration);

builder.Services.AddExceptionHandler<
    GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Frontend",
        policy =>
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy(
        AuthPolicies.LoginRateLimit,
        httpContext =>
        {
            var clientIp =
                httpContext.Connection.RemoteIpAddress?
                    .ToString()
                ?? "unknown";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: clientIp,
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder =
                        QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                });
        });

    options.OnRejected = async (
        context,
        cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode =
            StatusCodes.Status429TooManyRequests;

        context.HttpContext.Response.ContentType =
            "application/problem+json";

        var problem = new ProblemDetails
        {
            Status =
                StatusCodes.Status429TooManyRequests,
            Title = "Quá nhiều yêu cầu đăng nhập",
            Detail =
                "Bạn đã thử đăng nhập quá nhiều lần. Vui lòng thử lại sau một phút.",
            Type = "https://httpstatuses.com/429",
            Instance =
                context.HttpContext.Request.Path
        };

        problem.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;

        await context.HttpContext.Response
            .WriteAsJsonAsync(
                problem,
                cancellationToken);
    };
});

builder.Services.AddRemainingAdminModules();
builder.Services.AddPublicPortfolioModule();

builder.Services
    .AddOptions<EmailOptions>()
    .Bind(
        builder.Configuration.GetSection(
            EmailOptions.SectionName))
    .Validate(
        options =>
            !options.Enabled ||
            (
                !string.IsNullOrWhiteSpace(options.Host) &&
                options.Port > 0 &&
                !string.IsNullOrWhiteSpace(options.Username) &&
                !string.IsNullOrWhiteSpace(options.Password) &&
                !string.IsNullOrWhiteSpace(options.FromEmail) &&
                !string.IsNullOrWhiteSpace(
                    options.NotificationRecipient)
            ),
        "Cấu hình Email chưa đầy đủ.")
    .ValidateOnStart();

builder.Services.AddTransient<
    IEmailSender,
    SmtpEmailSender>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var logger = services
        .GetRequiredService<ILogger<Program>>();

    try
    {
        var seeder =
            services.GetRequiredService<DatabaseSeeder>();

        await seeder.InitialiseAsync();

        logger.LogInformation(
            "Migration và seed database thành công.");
    }
    catch (Exception exception)
    {
        logger.LogError(
            exception,
            "Migration hoặc seed database thất bại.");

        throw;
    }
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseCors("Frontend");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();