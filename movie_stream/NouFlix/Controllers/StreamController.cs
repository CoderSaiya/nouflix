using Microsoft.AspNetCore.Mvc;
using NouFlix.Services;

namespace NouFlix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StreamController(StreamService svc) : Controller
{
    [HttpGet("movies/{movieId:int}/master.m3u8")]
    public Task<IResult> MovieMaster([FromRoute] int movieId, CancellationToken ct)
        => svc.GetMovieMasterAsync(movieId, Request, ct);

    [HttpGet("movies/{movieId:int}/{quality}/index.m3u8")]
    public Task<IResult> MovieVariant([FromRoute] int movieId, [FromRoute] string quality, CancellationToken ct)
        => svc.GetMovieVariantAsync(movieId, quality, HttpContext, ct);

    [HttpGet("movies/{movieId:int}/{quality}/{file}")]
    public Task<IResult> MovieSegment([FromRoute] int movieId, [FromRoute] string quality, [FromRoute] string file, CancellationToken ct)
        => svc.GetMovieSegmentAsync(movieId, quality, file, HttpContext, ct);

    [HttpGet("movies/{movieId:int}/sub/{lang}/index.m3u8")]
    public Task<IResult> MovieSubIndex([FromRoute] int movieId, [FromRoute] string lang, CancellationToken ct)
        => svc.GetMovieSubIndexAsync(movieId, lang, HttpContext, ct);

    [HttpGet("movies/{movieId:int}/sub/{lang}/{file}")]
    public Task<IResult> MovieSubSeg([FromRoute] int movieId, [FromRoute] string lang, [FromRoute] string file, CancellationToken ct)
        => svc.GetMovieSubSegAsync(movieId, lang, file, HttpContext, ct);

    [HttpGet("movies/{movieId:int}/episodes/{episodeId:int}/master.m3u8")]
    public Task<IResult> EpisodeMaster([FromRoute] int movieId, [FromRoute] int episodeId, CancellationToken ct)
        => svc.GetEpisodeMasterAsync(movieId, episodeId, Request, ct);

    [HttpGet("movies/{movieId:int}/episodes/{episodeId:int}/{quality}/index.m3u8")]
    public Task<IResult> EpisodeVariant([FromRoute] int movieId, [FromRoute] int episodeId, [FromRoute] string quality, CancellationToken ct)
        => svc.GetEpisodeVariantAsync(movieId, episodeId, quality, HttpContext, ct);
    
    [HttpGet("movies/{movieId:int}/episodes/{episodeId:int}/{quality}/{file}")]
    public Task<IResult> EpisodeSegment([FromRoute] int movieId, [FromRoute] int episodeId, [FromRoute] string quality, [FromRoute] string file, CancellationToken ct)
        => svc.GetEpisodeSegmentAsync(movieId, episodeId, quality, file, HttpContext, ct);

    [HttpGet("movies/{movieId:int}/episodes/{episodeId:int}/sub/{lang}/index.m3u8")]
    public Task<IResult> EpisodeSubIndex([FromRoute] int movieId, [FromRoute] int episodeId, [FromRoute] string lang, CancellationToken ct)
        => svc.GetEpisodeSubIndexAsync(movieId, episodeId, lang, HttpContext, ct);

    [HttpGet("movies/{movieId:int}/episodes/{episodeId:int}/sub/{lang}/{file}")]
    public Task<IResult> EpisodeSubSeg([FromRoute] int movieId, [FromRoute] int episodeId, [FromRoute] string lang, [FromRoute] string file, CancellationToken ct)
        => svc.GetEpisodeSubSegAsync(movieId, episodeId, lang, file, HttpContext, ct);
}