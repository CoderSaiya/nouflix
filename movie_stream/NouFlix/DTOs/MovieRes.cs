namespace NouFlix.DTOs;

public record MovieRes(
    int Id,
    string Slug,
    string Title,
    string PosterUrl,
    string Type,
    string Status,
    float AvgRating,
    int ViewCount,
    DateTime? ReleaseDate,
    List<GenreDto.GenreRes> Genres
    );