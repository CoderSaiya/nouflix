using Microsoft.AspNetCore.Mvc;
using NouFlix.DTOs;
using NouFlix.Models.Common;
using NouFlix.Services;

namespace NouFlix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeasonController(SeasonService svc) : Controller
{
    [HttpGet("{movieId:int}")]
    public async Task<IActionResult> GetSeasonsAsync([FromRoute] int movieId, CancellationToken ct = default)
        => Ok(GlobalResponse<IEnumerable<SeasonRes>>.Success(await svc.GetAllSeasonsAsync(movieId, ct)));
    
    [HttpGet("{seasonNumber:int}/movie/{movieId:int}")]
    public async Task<IActionResult> GetEpisodeBySeason([FromRoute] int movieId, [FromRoute] int seasonNumber, CancellationToken ct = default)
        => Ok(GlobalResponse<IEnumerable<EpisodeRes>>.Success(await svc.GetEpisodeBySeason(movieId, seasonNumber, ct)));
}