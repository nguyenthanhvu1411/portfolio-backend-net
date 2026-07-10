using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Portfolio.Infrastructure.Persistence.Seed;

public static class DatabaseInitialisationExtensions
{
    public static async Task InitialisePortfolioDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.InitialiseAsync(cancellationToken);
    }
}
