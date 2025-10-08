using System.Linq.Expressions;

namespace NouFlix.Persistence.Repositories.Interfaces;

public interface IRepository<T> where T : class
{
    IQueryable<T> Query(bool asNoTracking = true);
    Task<T?> FindAsync(params object[] keys); // khoá chính
    Task<List<T>> GetByNameAsync(string name, bool asNoTracking = true);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    void RemoveRange(IEnumerable<T> entities);
    
    Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate = null,
        int? skip = null, int? take = null,
        CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default);
}