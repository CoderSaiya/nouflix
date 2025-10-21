using NouFlix.DTOs;
using NouFlix.Mapper;
using NouFlix.Models.Common;
using NouFlix.Models.Entities;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Services;

public class EpisodeService(IUnitOfWork uow)
{
    public async Task<IEnumerable<EpisodeRes>> GetEpisodeBySeason(int movieId, int seasonNumber, CancellationToken ct = default)
        => await (await uow.Episodes.GetByMovieAndSeasonNumberAsync(movieId, seasonNumber, ct)).ToEpisodeResListAsync(seasonNumber, ct);
    
    public async Task<IEnumerable<EpisodeRes>> GetEpisodeByMovie(int movieId, CancellationToken ct = default)
        => await (await uow.Episodes.GetByMovieAsync(movieId, ct: ct)).ToEpisodeResListAsync(0, ct);

    public async Task<IEnumerable<EpisodeRes>> GetBySeasonIds(int movieId, int[] seasonIds,
        CancellationToken ct = default)
    {
        var list = await uow.Episodes.GetByMovieAndSeasonIdsAsync(movieId, seasonIds, ct);
        var tasks = list.Select(e => e.ToEpisodeResAsync(e.SeasonId ?? 0, ct));
        return await Task.WhenAll(tasks);
    }
    
    public async Task<int> CreateAsync(EpisodeDto.UpsertEpisodeReq req, CancellationToken ct = default)
    {
        var newE = new Episode
        {
            MovieId = req.MovieId,
            SeasonId = req.SeasonId,
            Title = req.Title,
            ReleaseDate = req.ReleaseDate,
            Number = req.EpisodeNumber
        };
        
        await uow.Episodes.AddAsync(newE, ct);
        await uow.SaveChangesAsync(ct);

        return newE.Id;
    }

    public async Task UpdateAsync(int id, EpisodeDto.UpsertEpisodeReq req, CancellationToken ct = default)
    {
        var ep = await uow.Episodes.FindAsync(id);
        if(ep is null) throw new NotFoundException("episode", id);
        
        ep.Title = req.Title;
        ep.Number = req.EpisodeNumber;
        ep.Status = req.Status;
        ep.ReleaseDate = req.ReleaseDate;

        uow.Episodes.Update(ep);
        await uow.SaveChangesAsync(ct);
    }
    
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        if(await uow.Seasons.FindAsync(id) is not { } ss) 
            throw new NotFoundException("episode", id);
        
        uow.Seasons.Remove(ss);
    }
}