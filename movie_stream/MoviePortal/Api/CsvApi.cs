using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components.Forms;
using MoviePortal.Models.DTOs;

namespace MoviePortal.Api;

public class CsvApi(HttpClient http)
{
    private const string BaseUrl = "api/csv";
    private const long Max = 10 * 1024 * 1024;
    
    public async Task<List<SystemDto.EpisodeCsvPreviewRow>> PreviewEpisodeAsync(string csvText, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync($"{BaseUrl}/episode/preview", new { csvText }, ct);
        resp.EnsureSuccessStatusCode();

        var envelope = await resp.Content
            .ReadFromJsonAsync<Response<List<SystemDto.EpisodeCsvPreviewRow>>>(cancellationToken: ct);

        return envelope?.Data ?? new();
    }
    
    public async Task<List<SystemDto.EpisodeCsvPreviewRow>> PreviewFileAsync(IBrowserFile file, CancellationToken ct = default)
    {
        using var mp = new MultipartFormDataContent();
        
        await using var stream = file.OpenReadStream(Max, ct);
        var part = new StreamContent(stream);

        part.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(file.ContentType) ? "text/csv" : file.ContentType);
        
        mp.Add(part, "file", file.Name);

        using var resp = await http.PostAsync($"{BaseUrl}/episode/preview-file", mp, ct);
        resp.EnsureSuccessStatusCode();

        var envelope = await resp.Content
            .ReadFromJsonAsync<Response<List<SystemDto.EpisodeCsvPreviewRow>>>(cancellationToken: ct);

        return envelope?.Data ?? new();
    }
    
    public async Task<SystemDto.EpisodeCsvImportResult> ImportAsync(
        IEnumerable<SystemDto.EpisodeCsvPreviewRow> preview,
        bool overwrite,
        bool autoCreateSeason,
        CancellationToken ct = default)
    {
        var body = new
        {
            preview,
            overwrite,
            autoCreateSeason
        };

        var resp = await http.PostAsJsonAsync($"{BaseUrl}/episode/import", body, ct);
        resp.EnsureSuccessStatusCode();

        var envelope = await resp.Content
            .ReadFromJsonAsync<Response<SystemDto.EpisodeCsvImportResult>>(cancellationToken: ct);

        return envelope?.Data ?? new SystemDto.EpisodeCsvImportResult(0, 0, 0, 0);
    }
}