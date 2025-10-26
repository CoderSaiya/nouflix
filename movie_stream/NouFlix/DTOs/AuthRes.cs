namespace NouFlix.DTOs;

public record AuthRes(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    UserDto.UserRes User
    );