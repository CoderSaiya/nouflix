namespace NouFlix.DTOs;

public record MovieDetailRes(
    int Id,
    string Slug,
    string Title,
    string AlternateTitle,
    string Overview,
    string PosterUrl,
    string BackdropUrl,
    string ReleaseDate,
    int Runtime,
    float AvgRating,
    int VoteCount,
    int Popularity,
    List<GenreRes> Genres,
    string Country,
    string Language,
    string Status,
    bool Video,
    string Type,
    string Director
    );