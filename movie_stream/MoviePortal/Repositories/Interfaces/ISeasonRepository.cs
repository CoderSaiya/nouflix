using MoviePortal.Models.Entities;

namespace MoviePortal.Repositories.Interfaces;

public interface ISeasonRepository : IRepository<Season>
{
    Task<List<Season>> ListByMovieAsync(int movieId, CancellationToken ct = default);
    Task<int?> GetMaxNumberAsync(int movieId, CancellationToken ct = default);
    Task<bool> ExistsNumberAsync(int movieId, int number, CancellationToken ct = default);
}