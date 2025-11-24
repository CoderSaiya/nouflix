using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NouFlix.Models.Entities;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Persistence.Repositories;

public class UserRepository(AppDbContext ctx) : Repository<User>(ctx), IUserRepository
{
    private const string AI = "Vietnamese_100_CI_AI";
    
    public Task<List<User>> SearchAsync(string? keyword, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        IQueryable<User> query = Query()
            .Include(u => u.Profile)
            .Include(u => u.Histories)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var words = keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(EscapeLike)
                .Select(w => "%" + w + "%")
                .ToList();
            
            Console.Write(words.Count + " users found");
            
            if (words.Count > 0)
            {
                Expression<Func<User, bool>> predicate = _ => false;
                foreach (var p in words)
                {
                    Expression<Func<User, bool>> term = u =>
                        EF.Functions.Like(u.Email.Address, p) || 
                        (u.Profile.Name != null && 
                         (EF.Functions.Like(EF.Functions.Collate(u.Profile.Name.FirstName, AI), p) || 
                          EF.Functions.Like(EF.Functions.Collate(u.Profile.Name.LastName, AI), p)));
                    predicate = OrElse(predicate, term);
                }
                query = query.Where(predicate);
            }
        }

        return query
            .OrderByDescending(u => u.CreatedAt)
            .Skip(page)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(string? q, int page = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var query = Query();
        
        if (!string.IsNullOrWhiteSpace(q))
        {
            var words = q.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(EscapeLike)
                .Select(w => "%" + w + "%")
                .ToList();
            
            if (words.Count > 0)
            {
                Expression<Func<User, bool>> predicate = _ => false;
                foreach (var p in words)
                {
                    Expression<Func<User, bool>> term = u =>
                        EF.Functions.Like(u.Email.Address, p) || 
                        (u.Profile.Name != null && 
                         (EF.Functions.Like(EF.Functions.Collate(u.Profile.Name.FirstName, AI), p) || 
                          EF.Functions.Like(EF.Functions.Collate(u.Profile.Name.LastName, AI), p)));
                    predicate = OrElse(predicate, term);
                }
                query = query.Where(predicate);
            }
        }
        
        return query
            .Skip(page)
            .Take(pageSize)
            .CountAsync(ct);
    }
    
    private static string EscapeLike(string s) =>
        s.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
    
    static Expression<Func<T, bool>> OrElse<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
    {
        var param = Expression.Parameter(typeof(T), "m");
        var leftBody  = new ParameterReplace(left.Parameters[0],  param).Visit(left.Body)!;
        var rightBody = new ParameterReplace(right.Parameters[0], param).Visit(right.Body)!;
        return Expression.Lambda<Func<T, bool>>(Expression.OrElse(leftBody, rightBody), param);
    }
    
    sealed class ParameterReplace(ParameterExpression from, ParameterExpression to) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
            => ReferenceEquals(node, from) ? to : base.VisitParameter(node);
    }
    
    public async Task<User?> GetByEmailAsync(string email) =>
        await Db.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email.Address == email);

    public async Task<User?> GetByIdWithProfileAsync(Guid id) =>
        await Db.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id.Equals(id)); 

    public async Task<bool> EmailExistsAsync(string email) =>
        await Db.Users
            .AnyAsync(u => u.Email.Address == email);
}