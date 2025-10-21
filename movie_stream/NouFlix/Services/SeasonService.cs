using NouFlix.DTOs;
using NouFlix.Mapper;
using NouFlix.Models.Common;
using NouFlix.Models.Entities;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Services;

public class SeasonService(IUnitOfWork uow)
{
    public async Task<IEnumerable<SeasonRes>> GetAllSeasonsAsync(int movieId, CancellationToken ct = default)
        => await (await uow.Seasons.ListByMovieAsync(movieId, ct: ct)).ToSeasonResListAsync(ct);
    
    public async Task<SeasonRes> CreateAsync(int movieId, SeasonDto.CreateSeasonReq req, CancellationToken ct = default)
    {
        var newS = new Season
        {
            MovieId = movieId,
            Title = req.Title,
            Year = req.Year,
            Number = req.Number
        };
        
        await uow.Seasons.AddAsync(newS, ct);
        await uow.SaveChangesAsync(ct);

        return await newS.ToSeasonResAsync(ct);
    }

    public async Task<SeasonRes> UpdateAsync(int id, SeasonDto.UpdateSeasonReq req, CancellationToken ct = default)
    {
        var ss = await uow.Seasons.FindAsync(id);
        if(ss is null) throw new NotFoundException("season", id);
        
        ss.Title = req.Title;

        uow.Seasons.Update(ss);
        await uow.SaveChangesAsync(ct);

        return await ss.ToSeasonResAsync(ct);
    }
    
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        if(await uow.Seasons.FindAsync(id) is not { } ss) 
            throw new NotFoundException("season", id);
        
        uow.Seasons.Remove(ss);
    }
}