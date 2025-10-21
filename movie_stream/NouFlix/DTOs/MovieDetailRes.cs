using NouFlix.Models.ValueObject;

namespace NouFlix.DTOs;

public record MovieDetailRes(
    int Id,
    string Slug,
    string Title,
    string AlternateTitle,
    string Overview,
    string PosterUrl,
    string BackdropUrl,
    DateTime? ReleaseDate,
    string Director,
    int Runtime,
    float AvgRating,
    string AgeRating,
    int VoteCount,
    int Popularity,
    List<GenreRes> Genres,
    List<StudioRes> Studios,
    string Country,
    string Language,
    PublishStatus Status,
    MovieType Type,
    QualityLevel Quality,
    bool Video,
    bool IsVipOnly,
    DateTime CreatedAt,
    DateTime UpdatedAt
    );