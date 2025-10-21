using Microsoft.EntityFrameworkCore;
using NouFlix.Models.Entities;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Persistence.Repositories;

public class EpisodeRepository(AppDbContext db) : Repository<Episode>(db), IEpisodeRepository
{
    public Task<List<Episode>> GetByMovieAsync(int movieId, bool asNoTracking = true, CancellationToken ct = default)
        => Query(asNoTracking).Where(e => e.MovieId == movieId)
            .OrderBy(e => e.Number)
            .ToListAsync(ct);

    public Task<Episode?> GetByMovieAndNumberAsync(int movieId, int number, CancellationToken ct = default)
        => Query().FirstOrDefaultAsync(e => e.MovieId == movieId && e.Number == number, ct);
    
    public Task<List<Episode>> GetByMovieAndSeasonNumberAsync(int movieId, int seasonNumber, CancellationToken ct = default)
        => Query()
            .Include(e => e.Season)
            .Where(e => e.MovieId == movieId && e.Season!.Number == seasonNumber)
            .ToListAsync(ct);
    
    public Task<List<Episode>> GetByMovieAndSeasonIdsAsync(int movieId, int[] seasonId, CancellationToken ct = default)
        => Query()
            .Include(e => e.Season)
            .Where(e =>
                e.MovieId == movieId &&
                e.SeasonId.HasValue &&
                seasonId.Contains(e.SeasonId.Value))
            .ToListAsync(ct);

    public override Task<Episode?> FindAsync(params object[] keys)
    {
        if (keys[0] is not int id)
            throw new ArgumentException("FindAsync(Movie) cần 1 khóa kiểu int", nameof(keys));

        return Query()
            .AsSplitQuery()
            .Include(e => e.Season)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}