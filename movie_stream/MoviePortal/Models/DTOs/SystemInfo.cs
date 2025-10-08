namespace MoviePortal.Models.DTOs;

public sealed record SystemInfo(
    string Environment,
    string MachineName,
    string OSVersion,
    string Framework,
    string ProcessArchitecture,
    DateTime StartedAtUtc,
    TimeSpan Uptime);