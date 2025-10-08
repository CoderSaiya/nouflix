namespace MoviePortal.Models.DTOs;

public sealed record SystemHealthReport(
    SystemInfo Info,
    IReadOnlyList<HealthCheckResult> Checks);