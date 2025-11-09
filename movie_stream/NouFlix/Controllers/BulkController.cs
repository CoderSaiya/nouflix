using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NouFlix.DTOs;
using NouFlix.Models.Common;
using NouFlix.Services;

namespace NouFlix.Controllers;

[ApiController]
[Route("api/bulk/episodes")]
public class BulkController(BulkEpisodesService svc) : Controller
{
    [HttpGet("movies")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SearchMovies([FromQuery] string? q, CancellationToken ct)
    {
        var items = await svc.SearchMoviesAsync(q, ct);
        var data = items.Select(x => new SystemDto.MoviePick(x.Id, x.Title, x.Type)).ToList();
        return Ok(GlobalResponse<List<SystemDto.MoviePick>>.Success(data));
    }
    
    [HttpGet("movie/{movieId:int}/info")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> LoadMovieInfo([FromRoute] int movieId, CancellationToken ct)
    {
        var res = await svc.LoadMovieInfoAsync(movieId, ct);
        if (res is null) return NotFound(GlobalResponse<string>.Error("Movie not found"));

        var (movie, count, maxNumber) = res.Value;

        var info = new SystemDto.MovieInfoRes(
            Id: movie.Id,
            Title: movie.Title,
            Type: movie.Type,
            Status: movie.Status,
            Quality: movie.Quality,
            IsVipOnly: movie.IsVipOnly,
            ReleaseDate: movie.ReleaseDate,
            EpisodesCount: count,
            MaxNumber: maxNumber
        );

        return Ok(GlobalResponse<SystemDto.MovieInfoRes>.Success(info));
    }
    
    [HttpPost("plan")]
    [Authorize(Roles = "Admin")]
    [Consumes("application/json")]
    public async Task<IActionResult> BuildPlan([FromBody] SystemDto.BuildPlanReq? req, CancellationToken ct)
    {
        if (req is null) return BadRequest(GlobalResponse<string>.Error("Invalid body"));

        var rows = await svc.BuildPlanAsync(
            movieId: req.MovieId,
            start: req.Start,
            count: req.Count,
            titlePattern: req.TitlePattern ?? "Tập {n}",
            releaseStart: req.ReleaseStart,
            intervalDays: req.IntervalDays,
            ct
        );

        return Ok(GlobalResponse<List<PlanRow>>.Success(rows));
    }
    
    [HttpPost("create")]
    [Authorize(Roles = "Admin")]
    [Consumes("application/json")]
    public async Task<IActionResult> Create([FromBody] SystemDto.CreateReq req, CancellationToken ct)
    {
        if (!req.Plan.Any())
            return BadRequest(GlobalResponse<string>.Error("Plan is empty"));

        var (created, updated, skipped) = await svc.CreateAsync(
            movieId: req.MovieId,
            plan: req.Plan,
            overwrite: req.Overwrite,
            synopsis: req.Synopsis,
            durationMinutes: req.DurationMinutes,
            status: req.Status,
            quality: req.Quality,
            isVipOnly: req.IsVipOnly,
            ct
        );

        var result = new SystemDto.BulkCreateResult(created, updated, skipped);
        return Ok(GlobalResponse<SystemDto.BulkCreateResult>.Success(result));
    }
}