namespace NouFlix.DTOs;

public record EpisodeRes(
    int Id,
    int Number,
    int SeasonNumber,
    string Title,
    string Overview,
    DateTime? ReleaseDate,
    int Runtime
    );