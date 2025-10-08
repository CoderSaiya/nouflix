using NouFlix.DTOs;
using NouFlix.Models.Entities;

namespace NouFlix.Mapper;

public static class SeasonMapper
{
    public static async Task<SeasonRes> ToSeasonResAsync(
        this Season s,
        CancellationToken ct = default)
    {
        var episodeTasks = s.Episodes
            .Select(e => e.ToEpisodeResAsync(s.Number, ct));

        var episodeArray = await Task.WhenAll(episodeTasks);
        var episodes = episodeArray.ToList();

        return new SeasonRes(
            s.Id,
            s.Title,
            s.Number,
            s.Year?.ToString(),
            episodes.Count,
            episodes
        );
    }
    
    public static Task<SeasonRes[]> ToSeasonResListAsync(
        this IEnumerable<Season> seasons,
        CancellationToken ct = default)
        => Task.WhenAll(seasons.Select(s => s.ToSeasonResAsync(ct)));
}