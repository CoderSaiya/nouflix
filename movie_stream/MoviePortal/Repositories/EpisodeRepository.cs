using Microsoft.EntityFrameworkCore;
using MoviePortal.Data;
using MoviePortal.Models;
using MoviePortal.Repositories.Interfaces;

namespace MoviePortal.Repositories;

public class EpisodeRepository(AppDbContext db) : Repository<Episode>(db), IEpisodeRepository
{
    public Task<List<Episode>> GetByMovieAsync(int movieId, bool asNoTracking = true, CancellationToken ct = default)
        => Query(asNoTracking).Where(e => e.MovieId == movieId)
            .OrderBy(e => e.Number)
            .ToListAsync(ct);

    public Task<Episode?> GetByMovieAndNumberAsync(int movieId, int number, CancellationToken ct = default)
        => Query().FirstOrDefaultAsync(e => e.MovieId == movieId && e.Number == number, ct);
}