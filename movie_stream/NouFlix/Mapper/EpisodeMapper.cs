using NouFlix.DTOs;
using NouFlix.Models.Entities;

namespace NouFlix.Mapper;

public static class EpisodeMapper
{
    public static Task<EpisodeRes> ToEpisodeResAsync(
        this Episode e,
        int seasonNumber,
        CancellationToken ct = default)
        => Task.FromResult(new EpisodeRes(
            e.Id,
            e.Number,
            seasonNumber,
            e.Title,
            e.Synopsis,
            e.ReleaseDate,
            e.TotalDurationMinutes
        ));

    public static Task<EpisodeRes[]> ToEpisodeResListAsync(this IEnumerable<Episode> episodes, int seasonNumber, CancellationToken ct = default)
    => Task.WhenAll(episodes.Select(e => ToEpisodeResAsync(e, seasonNumber, ct)));
}