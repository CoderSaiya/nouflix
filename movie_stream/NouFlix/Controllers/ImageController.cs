using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NouFlix.DTOs;
using NouFlix.Mapper;
using NouFlix.Models.Common;
using NouFlix.Models.Entities;
using NouFlix.Models.Specification;
using NouFlix.Models.ValueObject;
using NouFlix.Services;

namespace NouFlix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController(
    AssetService svc,
    MinioObjectStorage storage,
    IOptions<StorageOptions> opts) : Controller
{
    [HttpGet("movie/{movieId:int}/{kind}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByKind([FromRoute] int movieId, [FromRoute] ImageKind kind)
    {
        var images = await svc.GetImageByKind(movieId, kind);
        return Ok(GlobalResponse<IEnumerable<AssetsDto.ImageAssetRes>>.Success(images));
    }

    [HttpPost("movie/{movieId:int}/{kind}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromRoute] int movieId, [FromRoute] ImageKind kind, IFormFile f)
    {
        var ext = Path.GetExtension(f.FileName);
        var bucket = opts.Value.Buckets.Images ?? "images";
        var key = $"movies/{movieId}/images/{kind.ToString().ToLowerInvariant()}/{Guid.NewGuid():N}{ext}";
        
        await using var s = f.OpenReadStream();
        var put = await storage.UploadAsync(bucket, s, key, f.ContentType); 
        
        var img = new ImageAsset
        {
            MovieId = movieId,
            Kind = kind,
            Alt = "",
            Bucket = bucket,
            ObjectKey = key,
            Endpoint = opts.Value.S3.Endpoint,
            ContentType = f.ContentType,
            SizeBytes = put.SizeBytes,
            ETag = put.ETag
        };

        await svc.CreateImageAsync(img);

        return Ok(GlobalResponse<AssetsDto.ImageAssetRes>.Success(await img.ToImageAssetResAsync(storage)));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await svc.DeleteImageAsync(id);
        return NoContent();
    }
}