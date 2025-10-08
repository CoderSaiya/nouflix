using MoviePortal.Models;
using MoviePortal.Repositories.Interfaces;

namespace MoviePortal.Services;

public class EpisodesService(IUnitOfWork uow)
{
    public Task<List<Episode>> ListByMovieAsync(int movieId, CancellationToken ct = default)
        => uow.Episodes.GetByMovieAsync(movieId, true, ct);

    public async Task<Episode> UpsertAsync(int movieId, int number, Action<Episode> apply, CancellationToken ct = default)
    {
        var ep = await uow.Episodes.GetByMovieAndNumberAsync(movieId, number, ct);
        if (ep is null)
        {
            ep = new Episode { MovieId = movieId, Number = number };
            await uow.Episodes.AddAsync(ep, ct);
        }
        apply(ep);
        await uow.SaveChangesAsync(ct);
        return ep;
    }

    public async Task<int> BulkCreateAsync(int movieId, int startNumber, int count, Action<int, Episode> init, bool overwrite, CancellationToken ct = default)
    {
        var created = 0;
        for (int i = 0; i < count; i++)
        {
            var n = startNumber + i;
            var ep = await uow.Episodes.GetByMovieAndNumberAsync(movieId, n, ct);
            if (ep is null)
            {
                ep = new Episode { MovieId = movieId, Number = n };
                init(n, ep);
                await uow.Episodes.AddAsync(ep, ct);
                created++;
            }
            else if (overwrite)
            {
                init(n, ep);
            }
        }
        await uow.SaveChangesAsync(ct);
        return created;
    }

    public async Task DeleteAsync(Episode ep, CancellationToken ct = default)
    {
        uow.Episodes.Remove(ep);
        await uow.SaveChangesAsync(ct);
    }
}