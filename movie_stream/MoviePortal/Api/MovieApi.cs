using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Components.Forms;
using MoviePortal.Models.DTOs;
using MoviePortal.Models.ValueObject;

namespace MoviePortal.Api;

public class MovieApi(HttpClient http)
{
    public async Task<SearchDto<List<MovieDto.MovieSummary>>> SearchAsync(
        string? q = "",
        int page = 0,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var sb = new StringBuilder("api/movie/search");
        var first = true;

        void AddStr(string key, string? val)
        {
            if (string.IsNullOrWhiteSpace(val)) return;
            sb.Append(first ? '?' : '&');
            sb.Append(key).Append('=').Append(Uri.EscapeDataString(val));
            first = false;
        }

        void AddInt(string key, int? val)
        {
            if (val.HasValue) AddStr(key, val.Value.ToString());
        }

        AddStr("q", q);
        AddInt("skip", page);
        AddInt("take", pageSize);

        var raw = await http.GetFromJsonAsync<Response<SearchDto<List<MovieDto.MovieSummary>>>>(sb.ToString(), ct);

        return raw!.Data!;
    }

    public async Task<MovieDto.Movie?> GetAsync(int id, CancellationToken ct = default)
        => (await http.GetFromJsonAsync<Response<MovieDto.Movie>>("api/movie/" + id, ct))?.Data;
    
    public async Task<int?> SaveAsync(MovieDto.UpsertMovieReq req, int id = 0, CancellationToken ct = default)
    {
        HttpResponseMessage resp;

        if (id == 0)
        {
            resp = await http.PostAsJsonAsync($"api/movie/{id}", req, ct);
            resp.EnsureSuccessStatusCode();

            var data = await resp.Content.ReadFromJsonAsync<Response<int?>>(ct);
            var newId = data?.Data;
            if (newId.HasValue) return newId.Value;

            return null;
        }
        else await http.PutAsJsonAsync($"api/movie/{id}", req, ct);

        return null;
    }

    public Task DeleteAsync(int id, CancellationToken ct = default)
    {
        http.DeleteAsync("api/movie/" + id, ct);
        return Task.CompletedTask;
    }

    public async Task<List<MovieDto.VideoAssets>> GetVideosAsync(int id, CancellationToken ct = default)
        => (await http.GetFromJsonAsync<Response<List<MovieDto.VideoAssets>>>($"api/video-assets/movie/{id}", ct))?.Data ?? [];

    public async Task<List<MovieDto.VideoAssets>> GetVideoByEpisode(int eId, CancellationToken ct = default)
        => (await http.GetFromJsonAsync<Response<List<MovieDto.VideoAssets>>>($"api/video-assets/episode/{eId}", ct))?.Data ?? [];

    public async Task<List<MovieDto.VideoAssets>> GetVideoByEpisodeIdsAsync(int[] ids, CancellationToken ct = default)
    {
        var mp = new MultipartFormDataContent();

        foreach (var id in ids)
            mp.Add(new StringContent(id.ToString()), "ids");
            
        
        using var resp = await http.PostAsync("api/video-assets/by-episodes", mp, ct);
        resp.EnsureSuccessStatusCode();
    
        var envelope = await resp.Content
            .ReadFromJsonAsync<Response<List<MovieDto.VideoAssets>>>(cancellationToken: ct);
        
        return envelope?.Data ?? new List<MovieDto.VideoAssets>();
    }
    
    public Task DeleteVideoAsync(int id, CancellationToken ct = default)
        => http.DeleteAsync($"api/video-assets/{id}", ct);

    public async Task<AssetsDto.Image?> UploadImageAsync(IBrowserFile file, int movieId,  ImageKind kind, CancellationToken ct = default)
    {
        const long maxSize = 20 * 1024 * 1024;
        
        var mp = new MultipartFormDataContent();
        
        await using var s = file.OpenReadStream(maxSize);
        
        var streamContent = new StreamContent(s);
        streamContent.Headers.ContentType =
            new MediaTypeHeaderValue(string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
        
        mp.Add(streamContent, "f", file.Name);
            
        
        using var resp = await http.PostAsync($"api/image/movie/{movieId}/{kind}", mp, ct);
        resp.EnsureSuccessStatusCode();
    
        var envelope = await resp.Content
            .ReadFromJsonAsync<Response<AssetsDto.Image>>(cancellationToken: ct); ;

        return envelope?.Data;
    }
    
    public Task DeleteImageAsync(int id, CancellationToken ct = default)
        => http.DeleteAsync($"api/image/{id}", ct);
    
    public async Task<List<MovieDto.Season>> GetSeasonsAsync(int id, CancellationToken ct = default)
        => (await http.GetFromJsonAsync<Response<List<MovieDto.Season>>>($"api/season/{id}", ct))?.Data ?? [];
    
    public async Task<MovieDto.Season> CreateSeasonAsync(int movieId, MovieDto.CreateSeasonReq req, CancellationToken ct=default)
        => (await (await http.PostAsJsonAsync($"api/season/movie/{movieId}", req, ct))
            .Content.ReadFromJsonAsync<Response<MovieDto.Season>>(cancellationToken: ct))?.Data!;
    
    public async Task<MovieDto.Season> UpdateSeasonAsync(int seasonId, MovieDto.UpdateSeasonReq req, CancellationToken ct=default)
        => (await (await http.PutAsJsonAsync($"api/season/{seasonId}", req, ct))
            .Content.ReadFromJsonAsync<Response<MovieDto.Season>>(cancellationToken: ct))?.Data!;

    public Task DeleteSeasonAsync(int seasonId, CancellationToken ct=default)
        => http.DeleteAsync($"api/seasons/{seasonId}", ct);

    public async Task<List<MovieDto.Episode>> GetEpisodeAsync(int movieId, int[] seasonIds, CancellationToken ct = default)
    {
        var mp = new MultipartFormDataContent();
        
        foreach (var id in seasonIds) 
            mp.Add(new StringContent(id.ToString()), "seasonIds");
        
        using var resp = await http.PostAsync($"api/episode/movie/{movieId}", mp, ct);
        resp.EnsureSuccessStatusCode();
    
        var envelope = await resp.Content
            .ReadFromJsonAsync<Response<List<MovieDto.Episode>>>(cancellationToken: ct);
        
        return envelope?.Data ?? new List<MovieDto.Episode>();
    }
    
    public async Task<List<MovieDto.Episode>> GetEpisodesByMovieAsync(int movieId, CancellationToken ct = default)
        => (await http.GetFromJsonAsync<Response<List<MovieDto.Episode>>>($"api/episode/{movieId}", ct))?.Data ?? [];

    public async Task<int> UpsertEpisodeAsync(MovieDto.UpsertEpisodeReq req, int id = 0, CancellationToken ct = default)
    {
        if (id == 0)
        {
            var resp = await http.PostAsJsonAsync($"api/episode/{id}", req, ct);
            resp.EnsureSuccessStatusCode();

            var data = await resp.Content.ReadFromJsonAsync<Response<int?>>(ct);
            var newId = data?.Data;
            if (newId.HasValue) return newId.Value;

            return 0;
        }
        else await http.PutAsJsonAsync("api/episode", req, ct);

        return 0;
    }

    public Task DeleteEpisodeAsync(int id, CancellationToken ct = default)
        => http.DeleteAsync($"api/episodes/{id}", ct);

    public async Task<List<AssetsDto.Image>> GetImageByKindAsync(int movieId, ImageKind kind,
        CancellationToken ct = default)
        => (await http.GetFromJsonAsync<Response<List<AssetsDto.Image>>>($"api/image/movie/{movieId}/{kind}", ct))?.Data ?? [];
}