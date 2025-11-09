using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NouFlix.DTOs;
using NouFlix.Models.Common;
using NouFlix.Services;

namespace NouFlix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StorageController(
    AssetService assetSvc
    ) : Controller
{
    [HttpGet("movie/{movieId:int}/poster")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetImageUrl([FromRoute] int movieId, CancellationToken ct = default)
    {
        var poster = await assetSvc.GetPosterAsync(movieId, ct);
        var url = poster.Url;
        return Ok(GlobalResponse<string>.Success(url));
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPreviewUrl([FromForm] AssetsDto.PreviewReq req, CancellationToken ct = default)
    {
        var uri = await assetSvc.GetPreviewAsync(req.Bucket, req.ObjectKey, ct);
        return Ok(GlobalResponse<string>.Success(uri.ToString()));
    }
}