using NouFlix.Models.Entities;

namespace NouFlix.Persistence.Repositories.Interfaces;

public interface IGenreRepository : IRepository<Genre>
{
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
}