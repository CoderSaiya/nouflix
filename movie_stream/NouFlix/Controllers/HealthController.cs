using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using NouFlix.DTOs;
using NouFlix.Models.Common;
using NouFlix.Models.ValueObject;
using NouFlix.Services;

namespace NouFlix.Controllers;

[ApiController]
[Route("api/system-health")]
public class HealthController(SystemHealthService health, IMemoryCache cache) : Controller
{
    private const string CacheKey = "system-health:report";
    
    [HttpGet]
    [Authorize("Admin")]
    [ResponseCache(Duration = 5, Location = ResponseCacheLocation.Client, NoStore = false)]
    public async Task<IActionResult> Get([FromQuery] bool fresh = false, CancellationToken ct = default)
    {
        SystemDto.SystemHealthReport? report;
        if (!fresh && cache.TryGetValue(CacheKey, out SystemDto.SystemHealthReport? cached))
        {
            report = cached;
        }
        else
        {
            report = await health.CheckAllAsync(ct);
            cache.Set(CacheKey, report, TimeSpan.FromSeconds(5));
        }

        var statusCode = report != null && report.Checks.Any(c => c.State == HealthState.Unhealthy) ? 503 : 200;
        return StatusCode(statusCode, GlobalResponse<SystemDto.SystemHealthReport>.Success(report));
    }
    
    [HttpGet("live")]
    [Authorize("Admin")]
    public IActionResult Live() => Ok("OK");
    
    [HttpGet("ready")]
    [Authorize("Admin")]
    public async Task<IActionResult> Ready(CancellationToken ct = default)
    {
        var report = await health.CheckAllAsync(ct);
        return report.Checks.Any(c => c.State == HealthState.Unhealthy)
            ? StatusCode(503, "Unhealthy")
            : Ok("Ready");
    }
    
    [HttpGet("checks/{name}")]
    [Authorize("Admin")]
    public async Task<IActionResult> RunOne([FromRoute] string name, CancellationToken ct = default)
    {
        var result = await health.CheckOneAsync(name, ct);
        if (result is null) return NotFound($"Unknown check: {name}");
        return Ok(GlobalResponse<SystemDto.HealthRes>.Success(result));
    }
}