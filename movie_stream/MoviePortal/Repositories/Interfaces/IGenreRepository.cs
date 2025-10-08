using MoviePortal.Models.Entities;

namespace MoviePortal.Repositories.Interfaces;

public interface IGenreRepository : IRepository<Genre>
{
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
}