namespace MoviePortal.Models.DTOs;

public sealed record Kpis(
    int TotalMovies, int PublishedMovies, int DraftMovies,
    int TotalEpisodes, int EpisodesMissingVideo,
    int TotalImages, int TotalVideos, int TotalAssets,
    long ImageBytes, long VideoBytes, long TotalBytes,
    int OrphanAssets);