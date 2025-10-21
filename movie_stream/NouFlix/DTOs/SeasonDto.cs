namespace NouFlix.DTOs;

public class SeasonDto
{
    public record CreateSeasonReq(int Number, string Title, int Year);
    public record UpdateSeasonReq(string Title);
}