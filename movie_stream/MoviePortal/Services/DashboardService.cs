using Microsoft.EntityFrameworkCore;
using MoviePortal.Data;
using MoviePortal.Models.DTOs;
using MoviePortal.Models.Entities;
using MoviePortal.Models.ValueObject;

namespace MoviePortal.Services;

public class DashboardService(AppDbContext db)
{

    public async Task<(Kpis, TaxonomyCounts, List<Movie>, List<Movie>, List<IssueItem>, List<ImageAsset>)>
        BuildAsync(CancellationToken ct = default)
    {
        // KPIs
        var totalMovies = await db.Movies.CountAsync(ct);
        var pubMovies = await db.Movies.CountAsync(m => m.Status == PublishStatus.Published, ct);
        var draftMovies = await db.Movies.CountAsync(m => m.Status == PublishStatus.Draft || m.Status == PublishStatus.InReview, ct);

        var totalEp = await db.Episodes.CountAsync(ct);
        var epMissVid = await db.Episodes.CountAsync(e => !db.VideoAssets.Any(v => v.EpisodeId == e.Id), ct);

        var totalImg = await db.ImageAssets.CountAsync(ct);
        var totalVid = await db.VideoAssets.CountAsync(ct);
        var imgBytes = await db.ImageAssets.SumAsync(i => (long?)(i.SizeBytes ?? 0), ct) ?? 0;
        var vidBytes = await db.VideoAssets.SumAsync(v => (long?)(v.SizeBytes ?? 0), ct) ?? 0;

        var orphanCount = await db.ImageAssets.CountAsync(i => i.MovieId == null && i.EpisodeId == null, ct)
                        + await db.VideoAssets.CountAsync(v => v.MovieId == null && v.EpisodeId == null, ct);

        var orphanSamples = await db.ImageAssets
            .Where(i => i.MovieId == null && i.EpisodeId == null)
            .OrderByDescending(i => i.Id).Take(5).AsNoTracking().ToListAsync(ct);

        var kpis = new Kpis(
            totalMovies, pubMovies, draftMovies, totalEp, epMissVid,
            totalImg, totalVid, totalImg + totalVid, imgBytes, vidBytes, imgBytes + vidBytes, orphanCount);

        // Taxonomy
        var taxo = new TaxonomyCounts(
            Genres: await db.Genres.CountAsync(ct),
            Studios: await db.Studios.CountAsync(ct));

        // Lists
        var recent = await db.Movies.AsNoTracking()
            .OrderByDescending(m => m.UpdatedAt).ThenByDescending(m => m.Id).Take(8).ToListAsync(ct);

        var topViews = await db.Movies.AsNoTracking()
            .OrderByDescending(m => m.ViewCount).ThenByDescending(m => m.Rating).Take(8).ToListAsync(ct);

        // Issues
        var issues = new List<IssueItem>();

        var missPoster = await db.Movies
            .Where(m => !db.ImageAssets.Any(i => i.MovieId == m.Id))
            .Select(m => m.Title).Take(5).ToListAsync(ct);
        if (missPoster.Count > 0)
            issues.Add(new("Phim thiếu poster", string.Join(", ", missPoster), "movies"));

        var seriesNoEp = await db.Movies
            .Where(m => m.Type == MovieType.Series && !db.Episodes.Any(e => e.MovieId == m.Id))
            .Select(m => m.Title).Take(5).ToListAsync(ct);
        if (seriesNoEp.Count > 0)
            issues.Add(new("Series chưa có tập", string.Join(", ", seriesNoEp), "movies"));

        var epMiss = await db.Episodes
            .Where(e => !db.VideoAssets.Any(v => v.EpisodeId == e.Id))
            .Select(e => new { e.MovieId, e.Number }).Take(5).ToListAsync(ct);
        if (epMiss.Count > 0)
            issues.Add(new("Tập thiếu video", string.Join(", ", epMiss.Select(e => $"Movie#{e.MovieId}-Ep{e.Number}")), "movies"));

        var badVideos = await db.VideoAssets
            .Where(v => v.ContentType == null || v.SizeBytes == null || v.SizeBytes == 0)
            .Take(5).CountAsync(ct);
        if (badVideos > 0)
            issues.Add(new("Video thiếu metadata", $"{badVideos} video thiếu ContentType/Size", "ops/storage"));

        return (kpis, taxo, recent, topViews, issues, orphanSamples);
    }
}