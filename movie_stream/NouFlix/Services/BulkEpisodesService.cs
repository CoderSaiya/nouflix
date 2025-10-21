using Microsoft.EntityFrameworkCore;
using NouFlix.DTOs;
using NouFlix.Models.Entities;
using NouFlix.Models.ValueObject;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Services;

public class BulkEpisodesService(IUnitOfWork uow)
{
    public async Task<List<(int Id, string Title, MovieType Type)>> SearchMoviesAsync(string? q, CancellationToken ct = default)
    {
        var query = uow.Movies.Query();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(m => EF.Functions.Like(m.Title, $"%{q.Trim()}%"));

        return await query
            .OrderByDescending(m => m.Type == MovieType.Series)
            .ThenBy(m => m.Title)
            .Select(m => new ValueTuple<int,string,MovieType>(m.Id, m.Title, m.Type))
            .Take(100).ToListAsync(ct);
    }

    public async Task<(Movie movie, int count, int maxNumber)?> LoadMovieInfoAsync(int movieId, CancellationToken ct = default)
    {
        var movie = await uow.Movies.FindAsync(movieId, ct);
        if (movie is null) return null;

        var count = await uow.Episodes.CountAsync(e => e.MovieId == movieId, ct);
        var max = await uow.Episodes.Query()
            .Where(e => e.MovieId == movieId)
            .Select(e => (int?)e.Number)
            .MaxAsync(ct) ?? 0;

        return (movie, count, max);
    }

    public async Task<List<PlanRow>> BuildPlanAsync(int movieId, int start, int count, string titlePattern, DateTime? releaseStart, int intervalDays, CancellationToken ct = default)
    {
        var plan = new List<PlanRow>(capacity: count);
        var numbers = Enumerable.Range(start, count).ToArray();

        var existed = await uow.Episodes.Query()
            .Where(e => e.MovieId == movieId && numbers.Contains(e.Number))
            .Select(e => new { e.Id, e.Number })
            .ToListAsync(ct);

        var map = existed.ToDictionary(x => x.Number, x => x.Id);

        for (int i = 0; i < count; i++)
        {
            var n = start + i;
            var title = (titlePattern ?? "Tập {n}").Replace("{n}", n.ToString());
            if (title.Length > 256) title = title[..256];
            DateTime? date = null;
            if (releaseStart.HasValue) date = releaseStart.Value.Date.AddDays((long)i * Math.Max(0, intervalDays));

            var exists = map.TryGetValue(n, out var epId);
            plan.Add(new PlanRow(n, title, date, exists, exists ? epId : null));
        }

        return plan;
    }

    public async Task<(int created, int updated, int skipped)> CreateAsync(
        int movieId, IEnumerable<PlanRow> plan, bool overwrite,
        string? synopsis, int? durationMinutes,
        PublishStatus status, QualityLevel quality, bool isVipOnly,
        CancellationToken ct = default)
    {
        int created = 0, updated = 0, skipped = 0;

        foreach (var p in plan)
        {
            if (p.Exists && !overwrite)
            {
                skipped++;
                continue;
            }

            Episode? ep;
            if (p.Exists && p.EpisodeId is not null)
            {
                ep = await uow.Episodes.FindAsync(p.EpisodeId.Value, ct);
                if (ep is null)
                {
                    skipped++;
                    continue;
                }
                updated++;
            }
            else
            {
                ep = new Episode
                {
                    MovieId = movieId,
                    Number = p.Number
                };
                await uow.Episodes.AddAsync(ep, ct);
                created++;
            }

            ep.Title = p.Title;
            ep.Synopsis = synopsis ?? ep.Synopsis;
            ep.Duration = durationMinutes.HasValue ? TimeSpan.FromMinutes(durationMinutes.Value) : ep.Duration;
            ep.ReleaseDate = p.ReleaseDate;
            ep.Status = status;
            ep.Quality = quality;
            ep.IsVipOnly = isVipOnly;
        }

        await uow.SaveChangesAsync(ct);
        return (created, updated, skipped);
    }
}