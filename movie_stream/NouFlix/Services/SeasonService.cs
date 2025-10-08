using NouFlix.DTOs;
using NouFlix.Mapper;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Services;

public class SeasonService(IUnitOfWork uow)
{
    public async Task<IEnumerable<SeasonRes>> GetAllSeasonsAsync(int movieId, CancellationToken ct = default)
        => await (await uow.Seasons.ListByMovieAsync(movieId, ct: ct)).ToSeasonResListAsync(ct);
    
    public async Task<IEnumerable<EpisodeRes>> GetEpisodeBySeason(int movieId, int seasonNumber, CancellationToken ct = default)
        => await (await uow.Episodes.GetByMovieAndSeasonNumberAsync(movieId, seasonNumber, ct)).ToEpisodeResListAsync(seasonNumber, ct);
}