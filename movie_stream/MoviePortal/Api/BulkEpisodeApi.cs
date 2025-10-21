using MoviePortal.Models.DTOs;

namespace MoviePortal.Api;

public class BulkEpisodeApi(HttpClient http)
{
    private const string BaseUrl = "api/bulk/episodes";
    
    public async Task<List<SystemDto.MoviePick>> SearchMoviesAsync(string? q, CancellationToken ct = default)
    {
        var url = string.IsNullOrWhiteSpace(q) ? $"{BaseUrl}/movies" : $"{BaseUrl}/movies?q={Uri.EscapeDataString(q)}";
        var env = await http.GetFromJsonAsync<Response<List<SystemDto.MoviePick>>>(url, ct);
        return env?.Data ?? new();
    }
    
    public async Task<SystemDto.MovieInfoRes?> LoadMovieInfoAsync(int movieId, CancellationToken ct = default)
    {
        var env = await http.GetFromJsonAsync<Response<SystemDto.MovieInfoRes>>($"{BaseUrl}/movie/{movieId}/info", ct);
        return env?.Data;
    }
    
    public async Task<List<SystemDto.PlanRow>> BuildPlanAsync(SystemDto.BuildPlanReq req, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync($"{BaseUrl}/plan", req, ct);
        resp.EnsureSuccessStatusCode();
        var env = await resp.Content.ReadFromJsonAsync<Response<List<SystemDto.PlanRow>>>(cancellationToken: ct);
        return env?.Data ?? new();
    }
    
    public async Task<SystemDto.BulkCreateResult> CreateAsync(SystemDto.CreateReq req, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync($"{BaseUrl}/create", req, ct);
        resp.EnsureSuccessStatusCode();
        var env = await resp.Content.ReadFromJsonAsync<Response<SystemDto.BulkCreateResult>>(cancellationToken: ct);
        return env?.Data ?? new SystemDto.BulkCreateResult(0, 0, 0);
    }
}