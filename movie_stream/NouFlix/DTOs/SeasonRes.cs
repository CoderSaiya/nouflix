namespace NouFlix.DTOs;

public record SeasonRes(
    int Id,
    string Title,
    int Number,
    string? Year,
    int EpisodeCount,
    List<EpisodeRes> Episodes
    );