namespace NouFlix.DTOs;

public record AuthRes(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    UserRes User
    );