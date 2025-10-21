using MoviePortal.Models.DTOs;

namespace MoviePortal.Api;

public class HealthApi(HttpClient http)
{
    public async Task<SystemDto.SystemHealthReport?> CheckAllAsync(CancellationToken ct = default)
    {
        var resp = await http.GetFromJsonAsync<Response<SystemDto.SystemHealthReport>>(
            "api/system-health?fresh=true", ct);
        return resp?.Data;
    }
}