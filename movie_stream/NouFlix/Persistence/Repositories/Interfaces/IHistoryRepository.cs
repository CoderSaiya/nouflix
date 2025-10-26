using NouFlix.Models.Entities;

namespace NouFlix.Persistence.Repositories.Interfaces;

public interface IHistoryRepository : IRepository<History>
{
    Task<IEnumerable<History>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<History?> GetAsync(Guid userId, int movieId, int? episodeId, CancellationToken ct = default);
    Task UpsertAsync(Guid userId, int movieId, int? episodeId, int positionSeconds, CancellationToken ct = default);
}