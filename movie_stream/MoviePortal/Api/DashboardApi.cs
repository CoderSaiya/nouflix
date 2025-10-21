using System.Text.Json;
using System.Text.Json.Serialization;
using MoviePortal.Models.DTOs;

namespace MoviePortal.Api;

public class DashboardApi(HttpClient http)
{
    private const string Base = "api/dashboard";

    public async Task<SystemDto.DashboardRes?> GetAsync(CancellationToken ct = default)
    {
        var opts = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        opts.Converters.Add(new JsonStringEnumConverter());

        var env = await http.GetFromJsonAsync<Response<SystemDto.DashboardRes>>(Base, opts, ct);
        return env?.Data;
    }
}