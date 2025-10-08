namespace NouFlix.DTOs;

public record UpdateProfileReq(
    string? FirstName = null,
    string? LastName = null,
    DateOnly? DateOfBirth = null,
    IFormFile? Avatar = null
    );