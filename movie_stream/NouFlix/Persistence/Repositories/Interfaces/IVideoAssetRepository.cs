using NouFlix.Models.Entities;

namespace NouFlix.Persistence.Repositories.Interfaces;

public interface IVideoAssetRepository : IRepository<VideoAsset>
{
    Task<IEnumerable<VideoAsset>> GetByMovieId(int movId);
    Task<IEnumerable<VideoAsset>> GetByEpisodeId(int epId);
}