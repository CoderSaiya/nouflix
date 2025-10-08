using MoviePortal.Models;

namespace MoviePortal.Repositories.Interfaces;

public interface IEpisodeRepository : IRepository<Episode>
{
    Task<List<Episode>> GetByMovieAsync(int movieId, bool asNoTracking = true, CancellationToken ct = default);
    Task<Episode?> GetByMovieAndNumberAsync(int movieId, int number, CancellationToken ct = default);
}