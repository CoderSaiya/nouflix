using NouFlix.DTOs;
using NouFlix.Models.Entities;

namespace NouFlix.Mapper;

public static class VideoAssetMapper
{
    public static Task<AssetsDto.VideoAssetRes> ToVideoAssetResAsync(
        this VideoAsset va,
        CancellationToken ct = default)
        => Task.FromResult(new AssetsDto.VideoAssetRes(va.Id, va.MovieId, va.EpisodeId ?? null, va.Kind, va.Quality, va.Bucket, va.ObjectKey));
    
    public static Task<AssetsDto.VideoAssetRes[]> ToVideoAssetResListAsync(
        this IEnumerable<VideoAsset> assets,
        CancellationToken ct = default)
        => Task.WhenAll(assets.Select(s => ToVideoAssetResAsync(s, ct)).ToArray());
}