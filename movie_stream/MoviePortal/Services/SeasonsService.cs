using Microsoft.EntityFrameworkCore;
using MoviePortal.Models;
using MoviePortal.Models.Entities;
using MoviePortal.Models.ValueObject;
using MoviePortal.Repositories.Interfaces;

namespace MoviePortal.Services;

public class SeasonsService(IUnitOfWork uow, MinioObjectStorage storage)
{
    public Task<List<Season>> ListAsync(int movieId, CancellationToken ct = default)
        => uow.Seasons.ListByMovieAsync(movieId, ct);

    public async Task<int> CreateAsync(
        int movieId, string? title = null, int? number = null,
        PublishStatus status = PublishStatus.Published,
        int? year = null,
        CancellationToken ct = default)
    {
        // đảm bảo movie tồn tại
        var movie = await uow.Movies.FindAsync(movieId, ct)
                    ?? throw new KeyNotFoundException("Movie not found");

        int next = number ?? ((await uow.Seasons.GetMaxNumberAsync(movieId, ct)) ?? 0) + 1;

        // nếu số mùa bị trùng → dịch các mùa >= next lên 1
        if (await uow.Seasons.ExistsNumberAsync(movieId, next, ct))
        {
            var toShift = await uow.Seasons.Query()
                               .Where(s => s.MovieId == movieId && s.Number >= next)
                               .OrderByDescending(s => s.Number) // giảm dần để chống unique conflict
                               .ToListAsync(ct);
            foreach (var s in toShift) s.Number += 1;
        }

        var season = new Season
        {
            MovieId = movieId,
            Number = next,
            Title = title ?? $"Season {next}",
            Year = year
        };

        await uow.Seasons.AddAsync(season, ct);
        await uow.SaveChangesAsync(ct);
        return season.Id;
    }

    public async Task RenameAsync(int seasonId, string title, CancellationToken ct = default)
    {
        var s = await uow.Seasons.FindAsync(seasonId, ct)
                ?? throw new KeyNotFoundException("Season not found");
        s.Title = title ?? "";
        uow.Seasons.Update(s);
        await uow.SaveChangesAsync(ct);
    }

    /// <summary>Đổi số mùa; nếu trùng thì tự dịch các mùa khác.</summary>
    public async Task ReorderAsync(int seasonId, int newNumber, CancellationToken ct = default)
    {
        var s = await uow.Seasons.FindAsync(seasonId, ct)
                ?? throw new KeyNotFoundException("Season not found");
        if (newNumber <= 0) throw new ArgumentOutOfRangeException(nameof(newNumber));

        if (s.Number == newNumber) return;
        var movieId = s.MovieId;
        var old = s.Number;

        // dịch dải [min(old,new) .. max(old,new)] theo hướng phù hợp
        if (newNumber > old)
        {
            // kéo các mùa (old+1..newNumber) xuống 1
            var affected = await uow.Seasons.Query()
                               .Where(x => x.MovieId == movieId && x.Number > old && x.Number <= newNumber)
                               .ToListAsync(ct);
            foreach (var a in affected) a.Number -= 1;
        }
        else
        {
            // đẩy các mùa (newNumber..old-1) lên 1
            var affected = await uow.Seasons.Query()
                               .Where(x => x.MovieId == movieId && x.Number >= newNumber && x.Number < old)
                               .ToListAsync(ct);
            foreach (var a in affected) a.Number += 1;
        }

        s.Number = newNumber;
        await uow.SaveChangesAsync(ct);
    }

    /// <summary>Xoá season: xoá episode + asset + object trên MinIO.</summary>
    public async Task DeleteAsync(int seasonId, CancellationToken ct = default)
    {
        // Lấy season + các episode id
        var s = await uow.Seasons.FindAsync(seasonId, ct)
                ?? throw new KeyNotFoundException("Season not found");

        var epIds = await uow.Episodes.Query()
                        .Where(e => e.SeasonId == seasonId)
                        .Select(e => e.Id)
                        .ToListAsync(ct);

        if (epIds.Count > 0)
        {
            // Lấy asset gắn vào tập
            var vids = await uow.VideoAssets.Query()
                            .Where(v => v.EpisodeId != null && epIds.Contains(v.EpisodeId.Value))
                            .ToListAsync(ct);
            var imgs = await uow.ImageAssets.Query()
                            .Where(i => i.EpisodeId != null && epIds.Contains(i.EpisodeId.Value))
                            .ToListAsync(ct);

            // Xoá object trên MinIO trước
            foreach (var v in vids) await storage.DeleteAsync(v.Bucket, v.ObjectKey, ct);
            foreach (var i in imgs) await storage.DeleteAsync(i.Bucket, i.ObjectKey, ct);

            // Xoá DB assets + episode
            uow.VideoAssets.RemoveRange(vids);
            uow.ImageAssets.RemoveRange(imgs);

            var eps = await uow.Episodes.Query().Where(e => epIds.Contains(e.Id)).ToListAsync(ct);
            uow.Episodes.RemoveRange(eps);
        }

        uow.Seasons.Remove(s);
        await uow.SaveChangesAsync(ct);

        // cân chỉnh lại Number cho các season sau nó (để liền mạch 1..N)
        var later = await uow.Seasons.Query()
                        .Where(x => x.MovieId == s.MovieId && x.Number > s.Number)
                        .OrderBy(x => x.Number)
                        .ToListAsync(ct);
        foreach (var a in later) a.Number -= 1;
        await uow.SaveChangesAsync(ct);
    }
    
    public async Task<int> AddEpisodeAsync(int seasonId, Episode e, CancellationToken ct = default)
    {
        var s = await uow.Seasons.FindAsync(seasonId, ct)
                ?? throw new KeyNotFoundException("Season not found");

        e.MovieId  = s.MovieId;
        e.SeasonId = s.Id;

        if (e.Number <= 0)
        {
            var next = await uow.Episodes.Query()
                         .Where(x => x.SeasonId == seasonId)
                         .Select(x => (int?)x.Number)
                         .MaxAsync(ct) ?? 0;
            e.Number = next + 1;
        }
        else
        {
            // nếu trùng số tập, dời các tập >= e.Number lên 1
            var duplicated = await uow.Episodes.Query()
                               .AnyAsync(x => x.SeasonId == seasonId && x.Number == e.Number, ct);
            if (duplicated)
            {
                var shift = await uow.Episodes.Query()
                               .Where(x => x.SeasonId == seasonId && x.Number >= e.Number)
                               .OrderByDescending(x => x.Number)
                               .ToListAsync(ct);
                foreach (var x in shift) x.Number += 1;
            }
        }

        await uow.Episodes.AddAsync(e, ct);
        await uow.SaveChangesAsync(ct);
        return e.Id;
    }
    
    public async Task MoveEpisodeAsync(int episodeId, int targetSeasonId, int? newNumber = null, CancellationToken ct = default)
    {
        var e = await uow.Episodes.FindAsync(episodeId, ct)
                ?? throw new KeyNotFoundException("Episode not found");
        var target = await uow.Seasons.FindAsync(targetSeasonId, ct)
                ?? throw new KeyNotFoundException("Target season not found");

        var oldSeasonId = e.SeasonId;
        var oldNumber = e.Number;

        e.SeasonId = targetSeasonId;
        e.MovieId  = target.MovieId;

        int number = newNumber ?? e.Number;
        if (number <= 0)
        {
            number = (await uow.Episodes.Query()
                          .Where(x => x.SeasonId == targetSeasonId)
                          .Select(x => (int?)x.Number)
                          .MaxAsync(ct) ?? 0) + 1;
        }
        else
        {
            // đẩy các tập đích
            var exists = await uow.Episodes.Query()
                            .AnyAsync(x => x.SeasonId == targetSeasonId && x.Number == number, ct);
            if (exists)
            {
                var shift = await uow.Episodes.Query()
                               .Where(x => x.SeasonId == targetSeasonId && x.Number >= number)
                               .OrderByDescending(x => x.Number)
                               .ToListAsync(ct);
                foreach (var x in shift) x.Number += 1;
            }
        }
        e.Number = number;

        await uow.SaveChangesAsync(ct);

        // dồn lại số tập ở mùa cũ
        if (oldSeasonId.HasValue)
        {
            var compact = await uow.Episodes.Query()
                              .Where(x => x.SeasonId == oldSeasonId.Value && x.Number > oldNumber)
                              .OrderBy(x => x.Number)
                              .ToListAsync(ct);
            foreach (var x in compact) x.Number -= 1;
            await uow.SaveChangesAsync(ct);
        }
    }
}