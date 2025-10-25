using NouFlix.DTOs;
using NouFlix.Mapper;
using NouFlix.Models.Common;
using NouFlix.Models.Entities;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Services;

public class TaxonomyService(IUnitOfWork uow)
{
    public async Task<IEnumerable<GenreDto.GenreRes>> SearchGenresAsync(string? q, CancellationToken ct = default)
        => await (await uow.Genres.GetByNameAsync(q ?? "", false)).ToGenreResListAsync(ct);
    
    public async Task<IEnumerable<GenreDto.GenreRes>> GenresAsync(CancellationToken ct = default)
        => await (await uow.Genres.ListAsync(null, null, null, ct)).ToGenreResListAsync(ct);
    
    public async Task<GenreDto.GenreRes?> GetGenreAsync(int id, CancellationToken ct = default)
        => await (await uow.Genres
                .FindAsync(id) ?? throw new NotFoundException("genre", id))
            .ToGenreResAsync(ct);

    public async Task SaveGenreAsync(string name, string? icon, int id = 0, CancellationToken ct = default)
    {
        Genre g;
        if (id == 0)
        {
            if (await uow.Genres.NameExistsAsync(name, null, ct)) 
                throw new InvalidOperationException("Tên thể loại đã tồn tại."); 
            
            g = new Genre { Name = name, Icon = icon };
            await uow.Genres.AddAsync(g, ct);
        }
        else
        {
            if (await uow.Genres.NameExistsAsync(name, id, ct)) 
                throw new InvalidOperationException("Tên thể loại đã tồn tại.");

            var existG = await uow.Genres.FindAsync(id);
            
            if (string.IsNullOrWhiteSpace(existG!.Name)) existG.Name = name;
            if (icon is not null) existG.Icon = icon;
            uow.Genres.Update(existG);
        }
        await uow.SaveChangesAsync(ct);
    }

    public async Task DeleteGenreAsync(int id, CancellationToken ct = default)
    {
        if(await uow.Genres.FindAsync(id) is not { } gen) 
            throw new NotFoundException("genre", id);
        
        uow.Genres.Remove(gen);
        await uow.SaveChangesAsync(ct);
    }
    
    public async Task<IEnumerable<StudioRes>> SearchStudiosAsync(string? q, CancellationToken ct = default)
        => await (await uow.Studios.GetByNameAsync(q ?? "", false)).ToStudioResListAsync(ct);
    
    public async Task<IEnumerable<StudioRes>> StudiosAsync(CancellationToken ct = default)
        => await (await uow.Studios.ListAsync(null, null, null, ct)).ToStudioResListAsync(ct);

    public async Task<StudioRes?> GetStudioAsync(int id, CancellationToken ct = default)
        => await (await uow.Studios
                .FindAsync(id) ?? throw new NotFoundException("studio", id))
            .ToStudioResAsync(ct);

    public async Task SaveStudioAsync(string name, int id = 0, CancellationToken ct = default)
    {
        Studio s;
        if (id == 0)
        {
            if (await uow.Genres.NameExistsAsync(name, null, ct)) 
                throw new InvalidOperationException("Tên thể loại đã tồn tại."); 
            
            s = new Studio { Name = name, };
            await uow.Studios.AddAsync(s, ct);
        }
        else
        {
            if (await uow.Studios.NameExistsAsync(name, id, ct)) 
                throw new InvalidOperationException("Tên thể loại đã tồn tại.");

            var existS = await uow.Studios.FindAsync(id);
            
            existS!.Name = name;
            uow.Studios.Update(existS);
        }
        await uow.SaveChangesAsync(ct);
    }

    public async Task DeleteStudioAsync(int id, CancellationToken ct = default)
    {
        if(await uow.Studios.FindAsync(id) is not { } stu) 
            throw new NotFoundException("studio", id);
        
        uow.Studios.Remove(stu);
        await uow.SaveChangesAsync(ct);
    }
}