using NouFlix.Models.Entities;

namespace NouFlix.Persistence.Repositories.Interfaces;

public interface IEpisodeRepository : IRepository<Episode>
{
    Task<List<Episode>> GetByMovieAsync(int movieId, bool asNoTracking = true, CancellationToken ct = default);
    Task<Episode?> GetByMovieAndNumberAsync(int movieId, int number, CancellationToken ct = default);
    Task<List<Episode>> GetByMovieAndSeasonNumberAsync(int movieId, int seasonNumber, CancellationToken ct = default);
    Task<List<Episode>> GetByMovieAndSeasonIdsAsync(int movieId, int[] seasonId, CancellationToken ct = default);
}