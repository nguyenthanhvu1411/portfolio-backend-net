namespace Portfolio.Infrastructure.Persistence.Seed;

internal static class PortfolioSeedIds
{
    internal static class Roles
    {
        public const int SuperAdmin = 1;
        public const int Admin = 2;
    }

    internal static class SkillCategories
    {
        public const int Backend = 1;
        public const int Frontend = 2;
        public const int Database = 3;
        public const int ToolsDevOps = 4;
        public const int Architecture = 5;
    }

    internal static class Skills
    {
        public const int CSharp = 1;
        public const int AspNetCore = 2;
        public const int EfCore = 3;
        public const int RestApi = 4;
        public const int JavaSpringBoot = 5;
        public const int ReactJs = 6;
        public const int TypeScript = 7;
        public const int JavaScript = 8;
        public const int TailwindCss = 9;
        public const int SqlServer = 10;
        public const int MySql = 11;
        public const int Git = 12;
        public const int Docker = 13;
        public const int CleanArchitecture = 14;
        public const int JwtRbac = 15;
    }

    internal static class Projects
    {
        public const int Wms = 1;
        public const int Ecommerce = 2;
        public const int Portfolio = 3;
    }
}
