namespace Portfolio.Infrastructure.Persistence.Seed;

public sealed class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public bool Enabled { get; set; } = true;
    public string Email { get; set; } = "admin@portfolio.com";
    public string Password { get; set; } = "Admin@123";
    public string FullName { get; set; } = "System Administrator";
}
