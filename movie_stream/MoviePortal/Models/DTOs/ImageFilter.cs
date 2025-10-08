using MoviePortal.Models.ValueObject;

namespace MoviePortal.Models.DTOs;


public sealed record ImageFilter(
    int? MovieId = null,
    int? EpisodeId = null,
    ImageKind? Kind = null,
    string? Q = null,
    int Take = 200
);