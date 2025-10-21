namespace MoviePortal.Models.DTOs;

public record SearchDto<T>(int? Count, T Data);