namespace MoviePortal.Models.DTOs;

public readonly record struct EpisodeCsvImportResult(int Created, int Updated, int Skipped, int Failed);