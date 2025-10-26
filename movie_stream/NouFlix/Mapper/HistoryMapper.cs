using NouFlix.DTOs;
using NouFlix.Models.Entities;

namespace NouFlix.Mapper;

public static class HistoryMapper
{
    private static Task<HistoryDto.Item> ToItemResAsync(
        this History h,
        CancellationToken ct = default)
        => Task.FromResult(new HistoryDto.Item(h.MovieId, h.WatchedDate, h.PositionSecond, h.IsCompleted));

    public static Task<HistoryDto.Item[]> ToItemListResAsync(
        this IEnumerable<History> list,
        CancellationToken ct = default)
        => Task.WhenAll(list.Select(h => ToItemResAsync(h, ct)).ToArray());
}