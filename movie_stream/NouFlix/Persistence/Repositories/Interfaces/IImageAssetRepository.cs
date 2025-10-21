using NouFlix.Models.Entities;
using NouFlix.Models.ValueObject;

namespace NouFlix.Persistence.Repositories.Interfaces;

public interface IImageAssetRepository : IRepository<ImageAsset>
{
    Task<List<ImageAsset>> GetByKind(int movieId, ImageKind kind);
}