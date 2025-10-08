namespace NouFlix.DTOs;

public record ExternalSignInResult(bool Succeeded, string? Error,
    string? AccessToken = null, DateTimeOffset? ExpiresAt = null,
    string? RefreshToken = null, object? User = null);