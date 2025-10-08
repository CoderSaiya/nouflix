using MoviePortal.Models.Entities;

namespace MoviePortal.Repositories.Interfaces;

public interface IStudioRepository : IRepository<Studio>
{
    Task<bool> NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
}