namespace Portfolio.Application.Auth.Models;

public sealed record AccessTokenResult(
    string Token,
    DateTime ExpiresAt);
