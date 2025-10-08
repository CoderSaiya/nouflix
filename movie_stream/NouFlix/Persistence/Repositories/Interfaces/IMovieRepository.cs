using NouFlix.Models.Entities;

namespace NouFlix.Persistence.Repositories.Interfaces;

public interface IMovieRepository : IRepository<Movie>
{
    Task<Movie?> GetBySlugAsync(string slug, bool asNoTracking = true);
    Task<List<Movie>> SearchAsync(string? q, int skip, int take, CancellationToken ct = default);
    Task<int> CountAsync(string? q, CancellationToken ct = default);
    Task<List<Movie>> TopByViewsAsync(int take, CancellationToken ct = default);
    Task<List<Movie>> FindCandidatesAsync(Movie seed, int max, bool includeVip, CancellationToken ct = default);
}