namespace NouFlix.DTOs;

public class StreamDto
{
    public record Position(int PositionSeconds, DateTime? LastWatched);
}