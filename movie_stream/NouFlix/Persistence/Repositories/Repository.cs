using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Persistence.Repositories;

public class Repository<T>(AppDbContext db) : IRepository<T>
    where T : class
{
    protected readonly AppDbContext Db = db;
    protected readonly DbSet<T> Set = db.Set<T>();

    public virtual IQueryable<T> Query(bool asNoTracking = true)
        => asNoTracking ? Set.AsNoTracking() : Set;

    public virtual Task<T?> FindAsync(params object[] keys)
        => Set.FindAsync(keys).AsTask();

    public virtual Task<List<T>> GetByNameAsync(string name, bool asNoTracking = true)
        => (asNoTracking ? Set.AsNoTracking() : Set)
            .Where(e => EF.Functions.Like(EF.Property<string>(e, "Name")!, $"%{name.Trim()}%"))
            .ToListAsync();

    public virtual Task AddAsync(T entity, CancellationToken ct = default)
        => Set.AddAsync(entity, ct).AsTask();

    public virtual Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
        => Set.AddRangeAsync(entities, ct);

    public virtual void Update(T entity) => Set.Update(entity);
    public virtual void Remove(T entity) => Set.Remove(entity);
    public virtual void RemoveRange(IEnumerable<T> entities) => Set.RemoveRange(entities);

    public virtual async Task<List<T>> ListAsync(System.Linq.Expressions.Expression<Func<T, bool>>? predicate = null, int? skip = null, int? take = null, CancellationToken ct = default)
    {
        var q = Query();
        if (predicate != null) q = q.Where(predicate);
        if (skip.HasValue) q = q.Skip(skip.Value);
        if (take.HasValue) q = q.Take(take.Value);
        return await q.ToListAsync(ct);
    }

    public virtual Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
        => predicate == null ? Set.CountAsync(ct) : Set.CountAsync(predicate, ct);
    
    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) =>
        await Db.Set<T>().AnyAsync(predicate);

    public virtual async Task<bool> ExistsAsync(int id) =>
        await Db.Set<T>().FindAsync(id) is not null;
    
    public virtual async Task<bool> ExistsAsync(Guid id) =>
        await Db.Set<T>().FindAsync(id) is not null;
}