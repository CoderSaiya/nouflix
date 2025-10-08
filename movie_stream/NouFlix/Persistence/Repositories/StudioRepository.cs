using Microsoft.EntityFrameworkCore;
using NouFlix.Models.Entities;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Persistence.Repositories;

public class StudioRepository(AppDbContext db) : Repository<Studio>(db), IStudioRepository
{
    public Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
        => Query().AnyAsync(s => s.Name == name && (excludeId == null || s.Id != excludeId.Value), ct);
}