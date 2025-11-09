using Microsoft.EntityFrameworkCore;
using NouFlix.Models.Entities;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Persistence.Repositories;

public class HistoryRepository(AppDbContext ctx) : Repository<History>(ctx), IHistoryRepository
{
    public async Task<IEnumerable<History>> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => await Query()
            .Where(h => h.UserId == userId)
            .ToListAsync(ct);

    public async Task<History?> GetAsync(Guid userId, int movieId, int? episodeId, CancellationToken ct = default)
        => await Query()
            .Include(h => h.Movie)
            .Include(h => h.Episode)
            .FirstOrDefaultAsync(h =>
                h.UserId == userId &&
                h.MovieId == movieId &&
                (episodeId != null && h.EpisodeId == episodeId.Value || episodeId == null),
                ct);

    public async Task UpsertAsync(
        Guid userId,
        int movieId,
        int? episodeId,
        int positionSeconds,
        CancellationToken ct = default)
    {
        var history = await GetAsync(userId, movieId, episodeId, ct);
        var now = DateTime.UtcNow;

        if (history is null)
        {
            history = new History
            {
                UserId = userId,
                MovieId = movieId,
                EpisodeId = episodeId,
            };
            
            await AddAsync(history, ct);
        }
        else
        {
            if (positionSeconds > history.PositionSecond)
                history.PositionSecond = positionSeconds;
            history.WatchedDate = now;
        }
    }
}