namespace NouFlix.DTOs;

public record UserRatingDto(
    string Name,
    string Username,
    string AvatarUrl,
    int Rating
    );