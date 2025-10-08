using Microsoft.EntityFrameworkCore;
using MoviePortal.Data;
using MoviePortal.Models.Entities;
using MoviePortal.Repositories.Interfaces;

namespace MoviePortal.Repositories;

public class MovieRepository(AppDbContext db) : Repository<Movie>(db), IMovieRepository
{
    public Task<Movie?> GetBySlugAsync(string slug, bool asNoTracking = true)
        => Query(asNoTracking).FirstOrDefaultAsync(m => m.Slug == slug);

    public Task<List<Movie>> SearchAsync(string? q, int skip, int take, CancellationToken ct = default)
    {
        var query = Query().OrderByDescending(m => m.UpdatedAt);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(m => EF.Functions.Like(m.Title, $"%{q.Trim()}%"))
                .OrderByDescending(m => m.UpdatedAt);
        return query.Skip(skip).Take(take).ToListAsync(ct);
    }

    public Task<int> CountAsync(string? q, CancellationToken ct = default)
    {
        var query = Query();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(m => EF.Functions.Like(m.Title, $"%{q.Trim()}%"));
        return query.CountAsync(ct);
    }

    public Task<List<Movie>> TopByViewsAsync(int take, CancellationToken ct = default)
        => Query().OrderByDescending(m => m.ViewCount).ThenByDescending(m => m.Rating)
            .Take(take).ToListAsync(ct);
    
    public override Task<Movie?> FindAsync(params object[] keys)
    {
        if (keys[0] is not int id)
            throw new ArgumentException("FindAsync(Movie) cần 1 khóa kiểu int", nameof(keys));

        return Set
            .AsNoTracking()
            .AsSplitQuery()
            .Include(m => m.MovieGenres)
            .Include(m => m.MovieStudios)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
}