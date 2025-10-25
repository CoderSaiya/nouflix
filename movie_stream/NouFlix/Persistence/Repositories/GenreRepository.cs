using Microsoft.EntityFrameworkCore;
using NouFlix.Models.Entities;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Persistence.Repositories;

public class GenreRepository(AppDbContext db) : Repository<Genre>(db), IGenreRepository
{
    public Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
        => Query()
            .Include(g => g.MovieGenres)
            .AnyAsync(g => g.Name == name && (excludeId == null || g.Id != excludeId.Value), ct);

    public override Task<List<Genre>> GetByNameAsync(string name, bool asNoTracking = true)
    {
        return Query(asNoTracking)
            .Include(g => g.MovieGenres)
            .Where(e => EF.Functions.Like(EF.Property<string>(e, "Name")!, $"%{name.Trim()}%"))
            .ToListAsync();
    }
}