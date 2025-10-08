using Microsoft.EntityFrameworkCore;
using MoviePortal.Data;
using MoviePortal.Models.Entities;
using MoviePortal.Repositories.Interfaces;

namespace MoviePortal.Repositories;

public class SeasonRepository(AppDbContext ctx) : Repository<Season>(ctx), ISeasonRepository
{
    public Task<List<Season>> ListByMovieAsync(int movieId, CancellationToken ct = default)
        => Db.Seasons
            .Where(m => m.MovieId == movieId)
            .OrderBy(s => s.Number)
            .AsNoTracking()
            .ToListAsync(ct);

    public Task<int?> GetMaxNumberAsync(int movieId, CancellationToken ct = default)
        => Db.Seasons
            .Where(s => s.MovieId == movieId)
            .Select(s => (int?)s.Number)
            .MaxAsync(ct);

    public Task<bool> ExistsNumberAsync(int movieId, int number, CancellationToken ct = default)
        => Db.Seasons.AnyAsync(s => s.MovieId == movieId && s.Number == number, ct);
}