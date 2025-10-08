namespace NouFlix.DTOs;

public record UserRes(
    Guid UserId,
    string Email,
    string? FirstName,
    string? LastName,
    string? Avatar,
    DateOnly? Dob,
    string Role,
    DateTime CreatedAt
    );