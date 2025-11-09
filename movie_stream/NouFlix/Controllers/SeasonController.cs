using Microsoft.AspNetCore.Authorization;
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
    [AllowAnonymous]
    public async Task<IActionResult> GetSeasonsAsync([FromRoute] int movieId, CancellationToken ct = default)
        => Ok(GlobalResponse<IEnumerable<SeasonRes>>.Success(await svc.GetAllSeasonsAsync(movieId, ct)));
    
    [HttpPost("movie/{movieId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromRoute] int movieId, [FromBody] SeasonDto.CreateSeasonReq req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var res = await svc.CreateAsync(movieId, req, ct);
        return CreatedAtAction(nameof(Create), GlobalResponse<SeasonRes>.Success(res));
    }
    
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] SeasonDto.UpdateSeasonReq req, CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var res = await svc.UpdateAsync(id, req, ct);
        return Ok(GlobalResponse<SeasonRes>.Success(res));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct = default)
    {
        await svc.DeleteAsync(id, ct);
        
        return NoContent();
    }
}