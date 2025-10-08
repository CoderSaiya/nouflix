using Microsoft.EntityFrameworkCore;
using MoviePortal.Data;
using MoviePortal.Models.Entities;
using MoviePortal.Repositories.Interfaces;

namespace MoviePortal.Repositories;

public class StudioRepository(AppDbContext db) : Repository<Studio>(db), IStudioRepository
{
    public Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default)
        => Query().AnyAsync(s => s.Name == name && (excludeId == null || s.Id != excludeId.Value), ct);
}