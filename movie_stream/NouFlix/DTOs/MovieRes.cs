namespace NouFlix.DTOs;

public record MovieRes(
    int Id,
    string Slug,
    string Title,
    string PosterUrl,
    float AvgRating,
    DateTime? ReleaseDate,
    List<GenreRes> Genres
    );