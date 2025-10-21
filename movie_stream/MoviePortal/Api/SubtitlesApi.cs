using Microsoft.AspNetCore.Components.Forms;
using MoviePortal.Models.DTOs;

namespace MoviePortal.Api;

public class SubtitlesApi(HttpClient http)
{
    public async Task<AssetsDto.SubtitleUploadRes?> UploadRawVttAsync(
        int movieId, int? episodeId, string lang, string label, IBrowserFile file, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(lang), "lang");
        content.Add(new StringContent(label), "label");

        // file content
        var stream = file.OpenReadStream(100 * 1024 * 1024);
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/vtt");
        content.Add(fileContent, "file", file.Name);

        var url = $"api/movies/{movieId}/subtitles/raw";
        if (episodeId is {} eid) url += $"?episodeId={eid}";

        using var resp = await http.PostAsync(url, content, ct);
        resp.EnsureSuccessStatusCode();

        var envelope = await resp.Content.ReadFromJsonAsync<Response<AssetsDto.SubtitleUploadRes>>(cancellationToken: ct)
                       ?? throw new InvalidOperationException("Empty response");

        if (!envelope.IsSuccess)
            throw new InvalidOperationException(envelope.Message ?? "Upload failed");

        return envelope.Data;
    }
}