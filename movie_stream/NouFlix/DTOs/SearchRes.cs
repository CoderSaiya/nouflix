namespace NouFlix.DTOs;

public record SearchRes<T>(int Count, T Data);