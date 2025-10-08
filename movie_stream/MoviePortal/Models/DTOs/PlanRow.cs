namespace MoviePortal.Models.DTOs;

public sealed record PlanRow(int Number, string Title, DateTime? ReleaseDate, bool Exists, int? EpisodeId);