using Microsoft.AspNetCore.Mvc;
using NouFlix.DTOs;
using NouFlix.Services;
using NouFlix.Services.Interface;

namespace NouFlix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranscodeController(
    MinioObjectStorage storage,
    IQueue<TranscodeDto.TranscodeJob> queue,
    IStatusStorage<TranscodeDto.TranscodeStatus> status) : Controller
{
    [HttpPost("upload")]
    [RequestSizeLimit(20L * 1024 * 1024 * 1024)] // 20GB body size
    [RequestFormLimits(MultipartBodyLengthLimit = 20L * 1024 * 1024 * 1024)] // 20GB cho multipart
    public async Task<IResult> UploadAndEnqueue(
        [FromForm] int movieId,
        [FromForm] int? episodeId,
        [FromForm] int? episodeNumber,
        [FromForm] int? seasonId,
        [FromForm] int? seasonNumber,
        [FromForm] string language,
        [FromForm] string[] profiles,
        IFormFile file,
        CancellationToken ct)
    {
        var jobId = Guid.NewGuid().ToString("N");

        status.Upsert(new TranscodeDto.TranscodeStatus
        {
            JobId = jobId,
            State = "Running",
            Progress = 0
        });


        // Lưu nguồn vào bucket uploads
        var bucket = "videos-src";
        var key = $"uploads/{movieId}/{(episodeId is null ? "" : $"ep{episodeNumber}/")}{jobId}/{file.FileName}";
        await using (var fs = file.OpenReadStream())
            await storage.UploadAsync(bucket, fs, key, file.ContentType ?? "application/octet-stream", ct);

        // Enqueue
        await queue.EnqueueAsync(new TranscodeDto.TranscodeJob{
            JobId = jobId,
            MovieId = movieId,
            EpisodeId = episodeId,
            EpisodeNumber = episodeNumber,
            SeasonId = seasonId,
            SeasonNumber = seasonNumber,
            SourceBucket = bucket,
            SourceKey = key,
            Profiles = (profiles?.Length ?? 0) > 0 ? profiles! : ["1080","720","480"],
            Language = string.IsNullOrWhiteSpace(language) ? "vi" : language
        }, ct);

        return Results.Accepted($"/api/transcode/{jobId}/status", new { jobId });
    }

    [HttpGet("{jobId}/status")]
    public IResult GetStatus([FromRoute] string jobId)
    {
        var st = status.Get(jobId);
        return st is null ? Results.NotFound() : Results.Ok(st);
    }
}