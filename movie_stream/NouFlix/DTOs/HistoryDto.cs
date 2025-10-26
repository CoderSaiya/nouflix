namespace NouFlix.DTOs;

public class HistoryDto
{
    public record Item(int MovieId, DateTime WatchedAt, int Position, bool IsCompleted);

    public record UpsertReq(int MovieId, int? EpisodeId, int Position);
}