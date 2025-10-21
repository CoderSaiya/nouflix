using System.Collections;
using NouFlix.Models.ValueObject;

namespace NouFlix.DTOs;

public record UpsertMovieReq(
    string Title,
    string AlternateTitle,
    string Slug,
    string Synopsis,
    string Director,
    string Country,
    string Language,
    string AgeRating,
    DateTime ReleaseDate,
    MovieType Type,
    PublishStatus Status,
    QualityLevel Quality,
    bool IsVipOnly,
    IEnumerable<int> GenreIds,
    IEnumerable<int> StudioIds
    );
    