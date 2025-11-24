using NouFlix.Models.Entities;

namespace NouFlix.Persistence.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<List<User>> SearchAsync(string? keyword, int page = 1, int pageSize = 50, CancellationToken ct = default);
    Task<int> CountAsync(string? q, int skip = 1, int pageSize = 10, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdWithProfileAsync(Guid id);
    Task<bool> EmailExistsAsync(string email);
}