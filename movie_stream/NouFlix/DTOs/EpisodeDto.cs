using NouFlix.Models.ValueObject;

namespace NouFlix.DTOs;

public class EpisodeDto
{
    public record UpsertEpisodeReq(int MovieId, int SeasonId, int EpisodeNumber, string Title, PublishStatus Status, DateTime ReleaseDate);
}