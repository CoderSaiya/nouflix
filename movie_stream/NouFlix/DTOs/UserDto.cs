namespace NouFlix.DTOs;

public class UserDto
{
    public record UserRes(
        Guid UserId,
        string Email,
        string? FirstName,
        string? LastName,
        string? Avatar,
        DateOnly? Dob,
        string Role,
        DateTime CreatedAt,
        List<HistoryDto.Item> WatchHistory
    );
}