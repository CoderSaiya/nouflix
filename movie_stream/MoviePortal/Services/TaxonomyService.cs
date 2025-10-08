using MoviePortal.Models.Entities;
using MoviePortal.Repositories.Interfaces;

namespace MoviePortal.Services;

public class TaxonomyService(IUnitOfWork uow)
{
    public Task<List<Genre>> SearchGenresAsync(string? q, CancellationToken ct = default)
        => uow.Genres.GetByNameAsync(q ?? "", false);
    
    public Task<List<Genre>> GenresAsync(CancellationToken ct = default)
        => uow.Genres.ListAsync(null, null, null, ct);
    
    public Task<Genre?> GetGenreAsync(int id, CancellationToken ct = default)
        => uow.Genres.FindAsync(id);

    public async Task<Genre> SaveGenreAsync(Genre g, CancellationToken ct = default)
    {
        if (g.Id == 0)
        {
            if (await uow.Genres.NameExistsAsync(g.Name, null, ct)) 
                throw new InvalidOperationException("Tên thể loại đã tồn tại.");
            await uow.Genres.AddAsync(g, ct);
        }
        else
        {
            if (await uow.Genres.NameExistsAsync(g.Name, g.Id, ct)) 
                throw new InvalidOperationException("Tên thể loại đã tồn tại.");
            uow.Genres.Update(g);
        }
        await uow.SaveChangesAsync(ct);
        return g;
    }

    public async Task DeleteGenreAsync(Genre g, CancellationToken ct = default)
    {
        uow.Genres.Remove(g);
        await uow.SaveChangesAsync(ct);
    }
    
    public Task<List<Studio>> SearchStudiosAsync(string? q, CancellationToken ct = default)
        => uow.Studios.GetByNameAsync(q ?? "", false);
    
    public Task<List<Studio>> StudiosAsync(CancellationToken ct = default)
        => uow.Studios.ListAsync(null, null, null, ct);

    public Task<Studio?> GetStudioAsync(int id, CancellationToken ct = default)
        => uow.Studios.FindAsync(id);

    public async Task<Studio> SaveStudioAsync(Studio s, CancellationToken ct = default)
    {
        if (s.Id == 0)
        {
            if (await uow.Studios.NameExistsAsync(s.Name, null, ct)) 
                throw new InvalidOperationException("Tên studio đã tồn tại.");
            await uow.Studios.AddAsync(s, ct);
        }
        else
        {
            if (await uow.Studios.NameExistsAsync(s.Name, s.Id, ct)) 
                throw new InvalidOperationException("Tên studio đã tồn tại.");
            uow.Studios.Update(s);
        }
        await uow.SaveChangesAsync(ct);
        return s;
    }

    public async Task DeleteStudioAsync(Studio s, CancellationToken ct = default)
    {
        uow.Studios.Remove(s);
        await uow.SaveChangesAsync(ct);
    }
}