using Microsoft.EntityFrameworkCore;
using NouFlix.DTOs;
using NouFlix.Mapper;
using NouFlix.Models.Entities;
using NouFlix.Models.ValueObject;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Services;

public class DashboardService(MinioObjectStorage storage, IUnitOfWork uow)
{

    public async Task<(SystemDto.Kpis, SystemDto.TaxonomyCounts, List<MovieRes>, List<MovieRes>, List<SystemDto.IssueItem>, List<ImageAsset>)>
        BuildAsync(CancellationToken ct = default)
    {
        // KPIs
        var totalMovies = await uow.Movies.CountAsync(ct: ct);
        var pubMovies = await uow.Movies.CountAsync(m => m.Status == PublishStatus.Published, ct);
        var draftMovies = await uow.Movies.CountAsync(m => m.Status == PublishStatus.Draft || m.Status == PublishStatus.InReview, ct);

        var totalEp = await uow.Episodes.CountAsync(ct: ct);
        var epMissVid = await uow.Episodes.CountAsync(e => !uow.VideoAssets.Query(false).Any(v => v.EpisodeId == e.Id), ct);

        var totalImg = await uow.ImageAssets.CountAsync(ct: ct);
        var totalVid = await uow.VideoAssets.CountAsync(ct: ct);
        var imgBytes = await uow.ImageAssets.Query(false).SumAsync(i => (long?)(i.SizeBytes ?? 0), ct) ?? 0;
        var vidBytes = await uow.VideoAssets.Query(false).SumAsync(v => (long?)(v.SizeBytes ?? 0), ct) ?? 0;

        var orphanCount = await uow.ImageAssets.CountAsync(i => i.MovieId == null && i.EpisodeId == null, ct)
                        + await uow.VideoAssets.CountAsync(v => v.MovieId == null && v.EpisodeId == null, ct);

        var orphanSamples = await uow.ImageAssets.Query()
            .Where(i => i.MovieId == null && i.EpisodeId == null)
            .OrderByDescending(i => i.Id)
            .Take(5)
            .ToListAsync(ct);

        var kpis = new SystemDto.Kpis(
            totalMovies, pubMovies, draftMovies, totalEp, epMissVid,
            totalImg, totalVid, totalImg + totalVid, imgBytes, vidBytes, imgBytes + vidBytes, orphanCount);

        // Taxonomy
        var taxo = new SystemDto.TaxonomyCounts(
            Genres: await uow.Genres.CountAsync(ct: ct),
            Studios: await uow.Studios.CountAsync(ct: ct));

        // Lists
        var recent = await (await uow.Movies.Query()
            .OrderByDescending(m => m.UpdatedAt)
            .ThenByDescending(m => m.Id)
            .Take(8)
            .ToListAsync(ct)).ToMovieResListAsync(storage, ct);

        var topViews = await (await uow.Movies.Query()
            .OrderByDescending(m => m.ViewCount)
            .ThenByDescending(m => m.Rating)
            .Take(8)
            .ToListAsync(ct)).ToMovieResListAsync(storage, ct);

        // Issues
        var issues = new List<SystemDto.IssueItem>();

        var missPoster = await uow.Movies.Query(false)
            .Where(m => !uow.ImageAssets.Query(false).Any(i => i.MovieId == m.Id))
            .Select(m => m.Title)
            .Take(5)
            .ToListAsync(ct);
        if (missPoster.Count > 0)
            issues.Add(new("Phim thiếu poster", string.Join(", ", missPoster), "movies"));

        var seriesNoEp = await uow.Movies.Query(false)
            .Where(m => m.Type == MovieType.Series && !uow.Episodes.Query(false).Any(e => e.MovieId == m.Id))
            .Select(m => m.Title)
            .Take(5)
            .ToListAsync(ct);
        if (seriesNoEp.Count > 0)
            issues.Add(new("Series chưa có tập", string.Join(", ", seriesNoEp), "movies"));

        var epMiss = await uow.Episodes.Query(false)
            .Where(e => !uow.VideoAssets.Query(false).Any(v => v.EpisodeId == e.Id))
            .Select(e => new { e.MovieId, e.Number })
            .Take(5)
            .ToListAsync(ct);
        if (epMiss.Count > 0)
            issues.Add(new("Tập thiếu video", string.Join(", ", epMiss.Select(e => $"Movie#{e.MovieId}-Ep{e.Number}")), "movies"));

        var badVideos = await uow.VideoAssets.Query(false)
            .Where(v => v.ContentType == null || v.SizeBytes == null || v.SizeBytes == 0)
            .Take(5)
            .CountAsync(ct);
        if (badVideos > 0)
            issues.Add(new("Video thiếu metadata", $"{badVideos} video thiếu ContentType/Size", "ops/storage"));

        return (kpis, taxo, recent.ToList(), topViews.ToList(), issues, orphanSamples);
    }
}