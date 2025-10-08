using Microsoft.EntityFrameworkCore;
using NouFlix.Models.Entities;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Persistence.Repositories;

public class GenreRepository(AppDbContext db) : Repository<Genre>(db), IGenreRepository
{
    public Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
        => Query().AnyAsync(g => g.Name == name && (excludeId == null || g.Id != excludeId.Value), ct);
}