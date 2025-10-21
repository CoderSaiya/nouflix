using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NouFlix.Models.Entities;
using NouFlix.Models.ValueObject;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Persistence.Repositories;

public class MovieRepository(AppDbContext db) : Repository<Movie>(db), IMovieRepository
{
    private const string AI = "Vietnamese_100_CI_AI";

    public override IQueryable<Movie> Query(bool asNoTracking = true)
    {
        return base
            .Query(asNoTracking)
            .Where(m => m.IsDeleted == false);
    }

    public Task<Movie?> GetBySlugAsync(string slug, bool asNoTracking = true)
        => Query(asNoTracking)
            .Include(m => m.Images)
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .Include(m => m.MovieStudios)
            .ThenInclude(ms => ms.Studio)
            .Include(m => m.Reviews)
            .FirstOrDefaultAsync(m => m.Slug == slug);

    public Task<List<Movie>> SearchAsync(string? q, int skip, int take, CancellationToken ct = default)
    {
        IQueryable<Movie> query = Query()
            .AsSplitQuery()
            .Include(m => m.Reviews)
            .Include(m => m.Images.Where(i => i.Kind == ImageKind.Poster))
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var words = q.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(EscapeLike)
                .Select(w => "%" + w + "%")
                .ToList();

            if (words.Count > 0)
            {
                Expression<Func<Movie, bool>> predicate = _ => false;
                foreach (var p in words)
                {
                    Expression<Func<Movie, bool>> term = m =>
                        EF.Functions.Like(EF.Functions.Collate(m.Title, AI), p) ||
                        EF.Functions.Like(EF.Functions.Collate(m.Slug, AI), p) ||
                        EF.Functions.Like(EF.Functions.Collate(m.Synopsis, AI), p);
                    predicate = OrElse(predicate, term);
                }
                query = query.Where(predicate);
            }
        }
        
        return query
            .OrderByDescending(m => m.UpdatedAt).ThenBy(m => m.Id)
            .Skip(skip * take)
            .Take(take)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(string? q, int skip, int take, CancellationToken ct = default)
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
                Expression<Func<Movie, bool>> predicate = _ => false;
                foreach (var p in words)
                {
                    Expression<Func<Movie, bool>> term = m =>
                        EF.Functions.Like(EF.Functions.Collate(m.Title, AI), p) ||
                        EF.Functions.Like(EF.Functions.Collate(m.Slug, AI), p) ||
                        EF.Functions.Like(EF.Functions.Collate(m.Synopsis, AI), p);
                    predicate = OrElse(predicate, term);
                }
                query = query.Where(predicate);
            }
        }
        
        return query.CountAsync(ct);
    }

    public Task<List<Movie>> TopByViewsAsync(int take, CancellationToken ct = default)
        => Query()
            .OrderByDescending(m => m.ViewCount)
            .ThenByDescending(m => m.Rating)
            .Include(m => m.Reviews)
            .Include(m => m.Images)
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .Take(take)
            .ToListAsync(ct);

    public override Task<Movie?> FindAsync(params object[] keys)
    {
        if (keys[0] is not int id)
            throw new ArgumentException("FindAsync(Movie) cần 1 khóa kiểu int", nameof(keys));

        return Set
            .AsNoTracking()
            .AsSplitQuery()
            .Include(m => m.Seasons)
            .ThenInclude(s => s.Episodes)
            .Include(m => m.Images)
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .Include(m => m.MovieStudios)
            .ThenInclude(ms => ms.Studio)
            .Include(m => m.Reviews)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public override async Task<List<Movie>> ListAsync(Expression<Func<Movie, bool>>? predicate = null, int? skip = null, int? take = null, CancellationToken ct = default)
    {
        var q = Query()
            .Include(m => m.Reviews)
            .Include(m => m.Images)
            .Include(m => m.MovieGenres)
            .ThenInclude(mg => mg.Genre)
            .AsNoTracking();
        
        if (predicate != null) q = q.Where(predicate);
        if (skip.HasValue) q = q.Skip(skip.Value);
        if (take.HasValue) q = q.Take(take.Value);
        
        return await q
            .ToListAsync(ct);
    }
    
    public async Task<List<Movie>> FindCandidatesAsync(Movie seed, int max, bool includeVip, CancellationToken ct = default)
    {
        var q = Db.Movies
            .AsNoTracking()
            .Where(m => !m.IsDeleted && m.Id != seed.Id && m.Status == PublishStatus.Published);
        if (!includeVip) q = q.Where(m => !m.IsVipOnly);

        // tập id để join
        var seedGenreIds = seed.MovieGenres.Select(g => g.GenreId).ToList();
        var seedStudioIds = seed.MovieStudios.Select(s => s.StudioId).ToList();
        var seedYear = seed.ReleaseDate?.Year;

        var sameDirector = string.IsNullOrWhiteSpace(seed.Director) ? q.Where(m => false) : q.Where(m => m.Director == seed.Director);
        var overlapGenre = seedGenreIds.Count == 0 ? q.Where(m => false) :
            q.Where(m => m.MovieGenres.Any(g => seedGenreIds.Contains(g.GenreId)));
        var overlapStudio = seedStudioIds.Count == 0 ? q.Where(m => false) :
            q.Where(m => m.MovieStudios.Any(s => seedStudioIds.Contains(s.StudioId)));
        var nearYear = seedYear is null ? q.Where(m => false) :
            q.Where(m => m.ReleaseDate.HasValue && Math.Abs(m.ReleaseDate.Value.Year - seedYear.Value) <= 5);

        return await sameDirector
            .Union(overlapGenre).Union(overlapStudio).Union(nearYear)
            .Distinct()
            .OrderByDescending(m => m.ViewCount) // pre-sort nhẹ
            .Take(max)
            .Include(m => m.MovieGenres)
            .Include(m => m.MovieStudios)
            .ToListAsync(ct);
    }

    public async Task<int> UpdateViewAsync(int movieId, int count = 1, CancellationToken ct = default)
    {
        var affected = await Db.Movies
            .Where(m => m.Id == movieId)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(m => m.ViewCount, m => m.ViewCount + count)
                , ct);

        return affected;
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
}