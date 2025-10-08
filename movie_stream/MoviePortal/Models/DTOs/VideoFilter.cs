using MoviePortal.Models.ValueObject;

namespace MoviePortal.Models.DTOs;

public sealed record VideoFilter(
    int? MovieId = null,
    int? EpisodeId = null,
    VideoKind? Kind = null,
    QualityLevel? Quality = null,
    string? Q = null,
    int Take = 200
);