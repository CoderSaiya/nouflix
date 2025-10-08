using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using NouFlix.DTOs;
using NouFlix.Models.Common;
using NouFlix.Models.Entities;
using NouFlix.Services;

namespace NouFlix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovieController(MovieService svc) : Controller
{
    [HttpGet("most-viewed")]
    public async Task<IActionResult> MostViewed([FromQuery, Range(1, 100)] int take = 12, CancellationToken ct = default)
    {
        var movies = await svc.GetMostViewed(take, ct);
        return Ok(GlobalResponse<IEnumerable<MovieRes>>.Success(movies));
    }
    
    [HttpGet("trending")]
    public async Task<IActionResult> Trending([FromQuery, Range(1, 100)] int take = 12, CancellationToken ct = default)
    {
        var movies = await svc.GetTrending(take, ct);
        return Ok(GlobalResponse<IEnumerable<MovieDetailRes>>.Success(movies));
    }
    
    [HttpGet("popular")]
    public async Task<IActionResult> Popular([FromQuery, Range(1, 100)] int take = 12, CancellationToken ct = default)
    {
        var movies = await svc.GetPopular(take, ct);
        return Ok(GlobalResponse<IEnumerable<MovieRes>>.Success(movies));
    }
    
    [HttpGet("most-rating")]
    public async Task<IActionResult> MostRating([FromQuery, Range(1, 100)] int take = 12, CancellationToken ct = default)
    {
        var movies = await svc.GetMostRating(take, ct);
        return Ok(GlobalResponse<IEnumerable<MovieRes>>.Success(movies));
    }
    
    [HttpGet("new")]
    public async Task<IActionResult> New([FromQuery, Range(1, 100)] int take = 12, CancellationToken ct = default)
    {
        var movies = await svc.GetNew(take, ct);
        return Ok(GlobalResponse<IEnumerable<MovieRes>>.Success(movies));
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetDetail([FromRoute] string slug, CancellationToken ct = default)
    {
        var movie = await svc.GetBySlug(slug, ct);
        
        return Ok(GlobalResponse<MovieDetailRes>.Success(movie));
    }
    
    [HttpGet("similar/{movieId:int}")]
    public async Task<IActionResult> GetSimilarity([FromRoute] int movieId, CancellationToken ct = default)
    {
        var movie = await svc.GetSimilar(movieId, ct: ct);
        
        return Ok(GlobalResponse<IReadOnlyList<MovieRes>>.Success(movie));
    }

    [HttpGet("id/{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id, CancellationToken ct = default)
    {
        var movie = await svc.GetById(id, ct);

        return Ok(GlobalResponse<MovieDetailRes>.Success(movie));
    }
}