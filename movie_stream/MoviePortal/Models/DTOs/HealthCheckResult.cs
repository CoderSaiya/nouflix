using MoviePortal.Models.ValueObject;

namespace MoviePortal.Models.DTOs;

public sealed record HealthCheckResult(
    string Name,
    HealthState State,
    string Message,
    long DurationMs);