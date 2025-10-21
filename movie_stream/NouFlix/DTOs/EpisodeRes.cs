using NouFlix.Models.ValueObject;

namespace NouFlix.DTOs;

public record EpisodeRes(
    int Id,
    int MovieId,
    int? SeasonId,
    int Number,
    int SeasonNumber,
    string Title,
    string Overview,
    DateTime? ReleaseDate,
    PublishStatus Status,
    int Runtime
    );