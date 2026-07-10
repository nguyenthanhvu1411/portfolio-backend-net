using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;

namespace Portfolio.Infrastructure.Persistence.Seed;

public sealed class DatabaseSeeder
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly AdminSeedOptions _options;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        ApplicationDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        IOptions<AdminSeedOptions> options,
        ILogger<DatabaseSeeder> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.Database.MigrateAsync(cancellationToken);
        await SeedAdminAsync(cancellationToken);
    }

    public async Task SeedAdminAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Admin seed đã bị tắt trong cấu hình.");
            return;
        }

        var email = _options.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(_options.Password))
        {
            throw new InvalidOperationException(
                "AdminSeed:Email và AdminSeed:Password không được để trống.");
        }

        var admin = await _dbContext.Users
            .Include(x => x.UserRoles)
            .SingleOrDefaultAsync(x => x.Email == email, cancellationToken);

        if (admin is null)
        {
            admin = new User
            {
                Email = email,
                FullName = _options.FullName.Trim(),
                Status = UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            admin.PasswordHash = _passwordHasher.HashPassword(admin, _options.Password);

            _dbContext.Users.Add(admin);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Đã tạo tài khoản quản trị {AdminEmail}.", email);
        }

        var superAdminRole = await _dbContext.Roles
            .SingleAsync(x => x.Name == "SuperAdmin", cancellationToken);

        var hasRole = await _dbContext.UserRoles.AnyAsync(
            x => x.UserId == admin.Id && x.RoleId == superAdminRole.Id,
            cancellationToken);

        if (!hasRole)
        {
            _dbContext.UserRoles.Add(new UserRole
            {
                UserId = admin.Id,
                RoleId = superAdminRole.Id
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
