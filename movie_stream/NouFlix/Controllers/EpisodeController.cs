using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NouFlix.DTOs;
using NouFlix.Models.Common;
using NouFlix.Services;

namespace NouFlix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EpisodeController(EpisodeService svc) : Controller
{
    [HttpGet("season/{seasonNumber:int}/movie/{movieId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEpisodeBySeason([FromRoute] int movieId, [FromRoute] int seasonNumber, CancellationToken ct = default)
        => Ok(GlobalResponse<IEnumerable<EpisodeRes>>.Success(await svc.GetEpisodeBySeason(movieId, seasonNumber, ct)));
    
    [HttpGet("movie/{movieId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEpisodeByMovie([FromRoute] int movieId, CancellationToken ct = default)
        => Ok(GlobalResponse<IEnumerable<EpisodeRes>>.Success(await svc.GetEpisodeByMovie(movieId, ct)));   
    
    [HttpPost("movie/{movieId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEpisodeBySeasonIds([FromRoute] int movieId, [FromForm] int[] seasonIds, CancellationToken ct = default)
        => Ok(GlobalResponse<IEnumerable<EpisodeRes>>.Success(await svc.GetBySeasonIds(movieId, seasonIds, ct)));
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] EpisodeDto.UpsertEpisodeReq req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var res = await svc.CreateAsync(req, ct);
        return CreatedAtAction(nameof(Create), GlobalResponse<int>.Success(res));
    }
    
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] EpisodeDto.UpsertEpisodeReq req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        await svc.UpdateAsync(id, req, ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct = default)
    {
        await svc.DeleteAsync(id, ct);
        
        return NoContent();
    }
}