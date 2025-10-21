using MoviePortal.Models.DTOs;

namespace MoviePortal.Api;

public class TranscodeApi(HttpClient http)
{
    public async Task<string> UploadAndEnqueueAsync(
        int movieId,
        int? episodeId,
        int? episodeNumber,
        int? seasonId,
        int? seasonNumber,
        string language,
        IEnumerable<string> profiles,
        Stream fileStream,
        string fileName,
        string? contentType = null,
        CancellationToken ct = default)
    {
        using var form = new MultipartFormDataContent();
        
        form.Add(new StringContent(movieId.ToString()), "movieId");
        
        if (episodeId.HasValue) 
            form.Add(new StringContent(episodeId.Value.ToString()), "episodeId");
        if (episodeNumber.HasValue)
            form.Add(new StringContent(episodeNumber.Value.ToString()), "episodeNumber");
        
        if (seasonId.HasValue) 
            form.Add(new StringContent(seasonId.Value.ToString()), "seasonId");
        if (seasonNumber.HasValue)
            form.Add(new StringContent(seasonNumber.Value.ToString()), "seasonNumber");
        
        form.Add(new StringContent(string.IsNullOrWhiteSpace(language) ? "vi" : language), "language");
        
        foreach (var p in profiles ?? [])
            form.Add(new StringContent(p), "profiles");

        var sc = new StreamContent(fileStream);
        if (!string.IsNullOrWhiteSpace(contentType)) sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        form.Add(sc, "file", fileName);

        var resp = await http.PostAsync("api/transcode/upload", form, ct);
        resp.EnsureSuccessStatusCode();
        var payload = await resp.Content.ReadFromJsonAsync<TranscodeDto.TranscodeEnqueueResponse>(cancellationToken: ct);
        return payload!.JobId;
    }

    public Task<TranscodeDto.TranscodeStatus?> GetStatusAsync(string jobId, CancellationToken ct = default)
        => http.GetFromJsonAsync<TranscodeDto.TranscodeStatus>($"api/transcode/{jobId}/status", ct);
}