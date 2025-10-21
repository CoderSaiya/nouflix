namespace MoviePortal.Models.DTOs;

public record Response<T>(
    T? Data,
    string Message,
    int StatusCode,
    bool IsSuccess);