using Microsoft.EntityFrameworkCore;
using NouFlix.Models.Entities;
using NouFlix.Models.ValueObject;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Persistence.Repositories;

public class ImageAssetRepository(AppDbContext db) : Repository<ImageAsset>(db), IImageAssetRepository
{
    public Task<List<ImageAsset>> GetByKind(int movieId, ImageKind kind)
        => Query()
            .Where(i => i.MovieId == movieId && i.Kind == kind)
            .ToListAsync();
}