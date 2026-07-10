namespace Portfolio.Application.Common.Security;

public static class AuthPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string SuperAdminOnly = "SuperAdminOnly";
    public const string LoginRateLimit = "AuthLoginRateLimit";

    public static readonly string[] AdminRoles = ["SuperAdmin", "Admin"];
}
