using MoviePortal.Models.DTOs;

namespace MoviePortal.Api;

public class StorageApi(HttpClient http)
{
    public async Task<string?> GetImageUrl(int movieId, CancellationToken ct = default)
        => (await http.GetFromJsonAsync<Response<string>>($"api/storage/movie/{movieId}/poster", ct))?.Data;

    public async Task<string> GetPreviewUrl(string bucket, string objectKey, CancellationToken ct = default)
    {
        var mp = new MultipartFormDataContent();
        
        mp.Add(new StringContent(bucket), "bucket");
        mp.Add(new StringContent(objectKey), "objectKey");
        
        using var resp = await http.PostAsync($"api/storage", mp, ct);
        resp.EnsureSuccessStatusCode();
    
        var envelope = await resp.Content
            .ReadFromJsonAsync<Response<string?>>(cancellationToken: ct);

        return envelope?.Data ?? "";
    }
}