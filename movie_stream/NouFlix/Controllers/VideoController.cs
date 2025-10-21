using Microsoft.AspNetCore.Mvc;
using NouFlix.DTOs;
using NouFlix.Models.Common;
using NouFlix.Services;

namespace NouFlix.Controllers;

[ApiController]
[Route("api/video-assets")]
public class VideoController(AssetService svc) : Controller
{
    [HttpGet("movie/{id:int}")]
    public async Task<IActionResult> GetVideoAssets([FromRoute] int id, CancellationToken ct = default)
    {
        var assets = await svc.GetVideoByMovieId(id, ct);
        return Ok(GlobalResponse<IEnumerable<AssetsDto.VideoAssetRes>>.Success(assets));
    }

    [HttpGet("episode/{id:int}")]
    public async Task<IActionResult> GetVideoByEpisode([FromRoute] int id, CancellationToken ct = default)
    {
        var assets = await svc.GetVideoByEpisodeId(id, ct);
        return Ok(GlobalResponse<IEnumerable<AssetsDto.VideoAssetRes>>.Success(assets));
    }
    
    [HttpPost("by-episodes")]
    public async Task<IActionResult> GetVideoByEpisodeIds([FromForm] int[] ids, CancellationToken ct = default)
    {
        var assets = await svc.GetVideoByEpisodeIds(ids, ct);
        return Ok(GlobalResponse<IEnumerable<AssetsDto.VideoAssetRes>>.Success(assets));
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct = default)
    {
        await svc.DeleteVideoAsync(id, ct);
        
        return NoContent();
    }
}