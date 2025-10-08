namespace NouFlix.DTOs;

public record ReviewRes(
    string Author,
    UserRatingDto AuthorDetails,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt
    );