using MoviePortal.Models.Entities;
using MoviePortal.Repositories.Interfaces;

namespace MoviePortal.Services;

public class MoviesService(IUnitOfWork uow)
{
    public async Task<Movie?> GetAsync(int id, CancellationToken ct = default)
    {
        var movie = await uow.Movies.FindAsync(id, ct);
        if (movie is null) return null;
        
        return movie;
    }
    
    public async Task<(int total, List<Movie> items)> SearchAsync(
        string? q, int skip, int take, CancellationToken ct = default)
    {
        // bảo vệ tham số
        skip = Math.Max(0, skip);
        take = Math.Clamp(take, 1, 200); // giới hạn page size nếu cần

        var total = await uow.Movies.CountAsync(q, ct);
        var items = await uow.Movies.SearchAsync(q, skip, take, ct);
        return (total, items);
    }
    
    public Task<int> CountAsync(string? q, CancellationToken ct = default)
        => uow.Movies.CountAsync(q, ct);
    
    public async Task<int> CreateAsync(Movie m, IEnumerable<int>? genreIds = null, IEnumerable<int>? studioIds = null, CancellationToken ct = default)
    {
        await uow.Movies.AddAsync(m, ct);

        // gán taxonomy nếu có
        if (genreIds is not null)
            foreach (var gid in genreIds) 
                m.MovieGenres.Add(new MovieGenre
                {
                    GenreId = gid,
                    Movie = m
                });
        
        if (studioIds is not null)
            foreach (var sid in studioIds) 
                m.MovieStudios.Add(new MovieStudio
                {
                    StudioId = sid,
                    Movie = m
                });

        await uow.SaveChangesAsync(ct);
        return m.Id;
    }
    
    public async Task UpdateAsync(Movie m, IEnumerable<int>? genreIds = null, IEnumerable<int>? studioIds = null, CancellationToken ct = default)
    {
        uow.Movies.Update(m);

        if (genreIds is not null)
        {
            m.MovieGenres.Clear();
            foreach (var gid in genreIds) m.MovieGenres.Add(new MovieGenre { GenreId = gid, MovieId = m.Id });
        }
        if (studioIds is not null)
        {
            m.MovieStudios.Clear();
            foreach (var sid in studioIds) m.MovieStudios.Add(new MovieStudio { StudioId = sid, MovieId = m.Id });
        }

        await uow.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Movie m, CancellationToken ct = default)
    {
        uow.Movies.Remove(m);
        await uow.SaveChangesAsync(ct);
    }
    
    static double Jaccard(IEnumerable<int> a, IEnumerable<int> b)
    {
        var setA = a as ISet<int> ?? new HashSet<int>(a);
        var setB = b as ISet<int> ?? new HashSet<int>(b);
        if (setA.Count == 0 && setB.Count == 0) return 0;
        int inter = setA.Intersect(setB).Count();
        int union = setA.Union(setB).Count();
        return union == 0 ? 0 : (double)inter / union; // 0..1
    }

    static double YearSimilarity(int y1, int y2)
    {
        var gap = Math.Abs(y1 - y2);
        return Math.Max(0, 1 - gap / 10.0); // cách 10 năm → 0
    }
    
    static string SimilarKey(int movieId, DateTime updatedAt, int topK, bool includeVip)
        => $"similar:movie:{movieId}:{updatedAt.Ticks}:{topK}:{includeVip}";
}