using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NouFlix.DTOs;
using NouFlix.Models.Common;
using NouFlix.Services;

namespace NouFlix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CsvController(CsvService svc) : Controller
{
    [HttpPost("episode/preview")]
    [Authorize("Admin")]
    [Consumes("application/json")]
    public async Task<IActionResult> PreviewEpisode([FromBody] SystemDto.EpisodeCsvPreviewReq req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.CsvText))
            return BadRequest("csvText is required.");

        var rows = await svc.BuildPreviewAsync(req.CsvText, ct);
        return Ok(GlobalResponse<List<SystemDto.EpisodeCsvPreviewRow>>.Success(rows));
    }
    
    [HttpPost("episode/preview-file")]
    [Authorize("Admin")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10MB (tuỳ chỉnh)
    public async Task<IActionResult> PreviewEpisodeFile(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest("File rỗng.");

        using var sr = new StreamReader(file.OpenReadStream(), detectEncodingFromByteOrderMarks: true);
        var text = await sr.ReadToEndAsync(ct);
        var rows = await svc.BuildPreviewAsync(text, ct);

        return Ok(GlobalResponse<List<SystemDto.EpisodeCsvPreviewRow>>.Success(rows));
    }
    
    [HttpPost("episode/import")]
    [Authorize("Admin")]
    [Consumes("application/json")]
    public async Task<IActionResult> ImportEpisode([FromBody] SystemDto.EpisodeCsvImportReq req, CancellationToken ct)
    {
        if (!req.Preview.Any())
            return BadRequest("preview is empty.");

        var result = await svc.ImportAsync(req.Preview, req.Overwrite, req.AutoCreateSeason, ct);
        return Ok(GlobalResponse<SystemDto.EpisodeCsvImportResult>.Success(result));
    }
}