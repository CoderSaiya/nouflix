using NouFlix.Models.ValueObject;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Services;

public class AccessService(IUnitOfWork uow)
{
    public async Task<bool> CanWatchMovieAsync(int movieId, string? userVipFlag, CancellationToken ct)
    {
        var mv = await uow.Movies.FindAsync(movieId, ct);
        if (mv is null) return false;
        if (mv.Status != PublishStatus.Published) return false;
        if (!mv.IsVipOnly) return true;
        return string.Equals(userVipFlag, "true", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<bool> CanWatchEpisodeAsync(int movieId, int episodeId, string? userVipFlag, CancellationToken ct)
    {
        var mv = await uow.Movies.FindAsync(movieId, ct);
        if (mv is null) return false;
        if (mv.Status != PublishStatus.Published) return false;
        if (mv.IsVipOnly && !string.Equals(userVipFlag, "true", StringComparison.OrdinalIgnoreCase)) return false;

        var ep = await uow.Episodes.FindAsync(episodeId, ct);
        if (ep is null || ep.MovieId != movieId) return false;
        // (tuỳ) kiểm tra ep.Status nếu bạn có
        return true;
    }
}