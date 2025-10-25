namespace NouFlix.DTOs;

public class GenreDto
{
    public record GenreRes(int Id, string Name, string? Icon, int MovieCount);

    public record SaveReq(string? Name, string? Icon);
}