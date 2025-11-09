using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NouFlix.DTOs;
using NouFlix.Models.Common;
using NouFlix.Services;

namespace NouFlix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaxonomyController(TaxonomyService svc) : Controller
{
    [HttpGet("genre/search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchGenre([FromQuery] string? q, CancellationToken ct)
        => Ok(GlobalResponse<IEnumerable<GenreDto.GenreRes>>.Success(await svc.SearchGenresAsync(q, ct)));

    [HttpGet("genre/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetGenre(int id, CancellationToken ct)
        => Ok(GlobalResponse<GenreDto.GenreRes>.Success(await svc.GetGenreAsync(id, ct)));
    
    [HttpPost("genre")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateGenre([FromBody] GenreDto.SaveReq req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Name)) 
            return BadRequest("Name is required.");

        await svc.SaveGenreAsync(req.Name, req.Icon ?? "\ud83c\udfac", 0, ct);
        return StatusCode(StatusCodes.Status201Created);
    }
    
    [HttpPut("genre/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateGenre([FromRoute] int id, [FromBody] GenreDto.SaveReq req, CancellationToken ct)
    {
        await svc.SaveGenreAsync(req.Name ?? "", req.Icon, id, ct);
        return NoContent();
    }
    
    [HttpDelete("genre/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteGenre([FromRoute] int id, CancellationToken ct)
    {
        await svc.DeleteGenreAsync(id, ct);
        return NoContent();
    }
    
    [HttpGet("studio/search")]
    [AllowAnonymous]
    public async Task<IActionResult> SearchStudio([FromQuery] string? q, CancellationToken ct)
        => Ok(GlobalResponse<IEnumerable<StudioRes>>.Success(await svc.SearchStudiosAsync(q, ct)));
    
    [HttpGet("studio/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStudio(int id, CancellationToken ct)
        => Ok(GlobalResponse<StudioRes>.Success(await svc.GetStudioAsync(id, ct)));
    
    [HttpPost("studio")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateStudio([FromBody] string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name)) 
            return BadRequest("Name is required.");

        await svc.SaveStudioAsync(name, 0, ct);
        return StatusCode(StatusCodes.Status201Created);
    }
    
    [HttpPut("studio/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required.");

        await svc.SaveStudioAsync(name, id, ct);
        return NoContent();
    }
    
    [HttpDelete("studio/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteStudio([FromRoute] int id, CancellationToken ct)
    {
        await svc.DeleteStudioAsync(id, ct);
        return NoContent();
    }
}