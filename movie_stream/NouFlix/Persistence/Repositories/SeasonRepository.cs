using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NouFlix.Models.Entities;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Persistence.Repositories;

public class SeasonRepository(AppDbContext context) : Repository<Season>(context), ISeasonRepository
{
    public Task<List<Season>> ListByMovieAsync(int movieId, CancellationToken ct = default)
        => Db.Seasons
            .Include(m => m.Episodes)
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