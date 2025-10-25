using NouFlix.Models.Entities;

namespace NouFlix.Persistence.Repositories.Interfaces;

public interface IMovieRepository : IRepository<Movie>
{
    Task<Movie?> GetBySlugAsync(string slug, bool asNoTracking = true);
    Task<List<Movie>> SearchAsync(string? q, int skip, int take, CancellationToken ct = default);
    Task<int> CountAsync(string? q, int skip, int take, CancellationToken ct = default);
    Task<List<Movie>> TopByViewsAsync(int take, CancellationToken ct = default);
    Task<List<Movie>> FindCandidatesAsync(Movie seed, int max, bool includeVip, CancellationToken ct = default);
    Task<List<Movie>> GetByGenreAsync(int genreId, CancellationToken ct = default);
    Task<int> UpdateViewAsync(int movieId, int count = 1, CancellationToken ct = default);
}