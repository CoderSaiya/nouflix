using Microsoft.EntityFrameworkCore;
using MoviePortal.Data;
using MoviePortal.Models.Entities;
using MoviePortal.Repositories.Interfaces;

namespace MoviePortal.Repositories;

public class GenreRepository(AppDbContext db) : Repository<Genre>(db), IGenreRepository
{
    public Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
        => Query().AnyAsync(g => g.Name == name && (excludeId == null || g.Id != excludeId.Value), ct);
}