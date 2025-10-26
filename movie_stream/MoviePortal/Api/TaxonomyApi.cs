using System.Text;
using MoviePortal.Models.DTOs;

namespace MoviePortal.Api;

public class TaxonomyApi(HttpClient http)
{
    public async Task<List<MovieDto.Genre>> SearchGenresAsync(
        string? q,
        CancellationToken ct = default)
    {
        var sb = new StringBuilder(
            $"api/taxonomy/genre/search{(!string.IsNullOrWhiteSpace(q) ? $"?q={Uri.EscapeDataString(q)}" : "")}"
        );
        
        var raw = await http.GetFromJsonAsync<Response<List<MovieDto.Genre>>>(sb.ToString(), ct);
        
        return raw!.Data!;
    }
    
    public async Task<MovieDto.Genre> GetGenreAsync(
        int id,
        CancellationToken ct = default)
    {
        var sb = new StringBuilder(
            $"api/taxonomy/genre/{id}"
        );
        
        var raw = await http.GetFromJsonAsync<Response<MovieDto.Genre>>(sb.ToString(), ct);
        
        return raw!.Data!;
    }

    public Task SaveGenreAsync(MovieDto.Genre g, CancellationToken ct = default)
    {
        if (g is null) throw new ArgumentNullException(nameof(g));
        if (string.IsNullOrWhiteSpace(g.Name)) throw new ArgumentException("Name is required.", nameof(g.Name));

        var req = new MovieDto.UpsertGenreReq(g.Name, g.Icon ?? null);
            
        if (g.Id == 0) http.PostAsJsonAsync("api/taxonomy/genre", req, ct);
        else http.PutAsJsonAsync($"api/taxonomy/genre/{g.Id}", req, ct);
        
        return Task.CompletedTask;
    }
    
    public Task DeleteGenreAsync(int id, CancellationToken ct = default)
    {
        http.DeleteAsync("api/taxonomy/genre/" + id, ct);
        return Task.CompletedTask;
    }
    
    public async Task<List<MovieDto.Studio>> SearchStudiosAsync(
        string? q,
        CancellationToken ct = default)
    {
        var sb = new StringBuilder(
            $"api/taxonomy/studio/search{(!string.IsNullOrWhiteSpace(q) ? $"?q={Uri.EscapeDataString(q)}" : "")}"
        );
        
        var raw = await http.GetFromJsonAsync<Response<List<MovieDto.Studio>>>(sb.ToString(), ct);
        
        return raw!.Data!;
    }
    
    public async Task<MovieDto.Studio> GetStudioAsync(
        int id,
        CancellationToken ct = default)
    {
        var sb = new StringBuilder(
            $"api/taxonomy/studio/{id}"
        );
        
        var raw = await http.GetFromJsonAsync<Response<MovieDto.Studio>>(sb.ToString(), ct);
        
        return raw!.Data!;
    }

    public Task SaveStudioAsync(MovieDto.Studio g, CancellationToken ct = default)
    {
        if (g is null) throw new ArgumentNullException(nameof(g));
        if (string.IsNullOrWhiteSpace(g.Name)) throw new ArgumentException("Name is required.", nameof(g.Name));
        
        if (g.Id == 0) http.PostAsJsonAsync("api/taxonomy/studio", g.Name, ct);
        else http.PutAsJsonAsync($"api/taxonomy/studio/{g.Id}", g.Name, ct);
        
        return Task.CompletedTask;
    }
    
    public Task DeleteStudioAsync(int id, CancellationToken ct = default)
    {
        http.DeleteAsync("api/taxonomy/studio/" + id, ct);
        return Task.CompletedTask;
    }
}