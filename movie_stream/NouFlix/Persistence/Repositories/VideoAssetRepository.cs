using Microsoft.EntityFrameworkCore;
using NouFlix.Models.Entities;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Persistence.Repositories;

public class VideoAssetRepository(AppDbContext db) : Repository<VideoAsset>(db), IVideoAssetRepository
{
    public async Task<IEnumerable<VideoAsset>> GetByMovieId(int movId)
        => await Db.VideoAssets
            .Where(a => a.MovieId == movId)
            .ToListAsync();

    public async Task<IEnumerable<VideoAsset>> GetByEpisodeId(int epId)
        => await Db.VideoAssets
            .Where(a => a.EpisodeId == epId)
            .ToListAsync();
}