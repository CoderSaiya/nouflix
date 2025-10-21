using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using NouFlix.Adapters;
using NouFlix.Helpers;
using NouFlix.Models.Specification;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Services;

public class StreamService(
    AccessService access,
    MinioObjectStorage storage,
    IHttpClientFactory http,
    IOptions<StorageOptions> opts,
    IUnitOfWork uow,
    ViewCounter counter
    )
{
    private string Bucket => opts.Value.Buckets.Videos ?? "videos";
    private static string BasePrefix(int movieId, int? seasonsNumber = null, int? episodeNumber = null)
        => seasonsNumber is not null && episodeNumber is not null ? $"hls/movies/{movieId}/ss{seasonsNumber}/ep{episodeNumber}" : $"hls/movies/{movieId}";
    
    public async Task<IResult> GetMovieMasterAsync(int movieId, HttpRequest req, HttpResponse res, CancellationToken ct)
    {
        // auth/authorize
        var vip = req.HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "vip")?.Value;
        if (!await access.CanWatchMovieAsync(movieId, vip, ct))
            return Results.StatusCode((int)HttpStatusCode.Forbidden);
        
        var key = $"{BasePrefix(movieId)}/master.m3u8";
        var text = await DownloadTextAsync(key, ct);
        var rewritten = RewriteMaster(text, req, movieId, null);
        return Results.Text(rewritten, "application/vnd.apple.mpegurl", Encoding.UTF8);
    }

    public async Task<IResult> GetMovieVariantAsync(int movieId, string quality, HttpContext ctx, CancellationToken ct)
    {
        var vip = ctx.User?.Claims?.FirstOrDefault(c => c.Type == "vip")?.Value;
        if (!await access.CanWatchMovieAsync(movieId, vip, ct))
            return Results.StatusCode((int)HttpStatusCode.Forbidden);

        var key = $"{BasePrefix(movieId)}/{quality}/index.m3u8";
        await ProxyObjectAsync(ctx, key, "application/vnd.apple.mpegurl", TimeSpan.FromMinutes(10), ct);
        return Results.Empty;
    }

    public async Task<IResult> GetMovieSegmentAsync(int movieId, string quality, string file, HttpContext ctx, CancellationToken ct)
    {
        var vip = ctx.User?.Claims?.FirstOrDefault(c => c.Type == "vip")?.Value;
        if (!await access.CanWatchMovieAsync(movieId, vip, ct))
            return Results.StatusCode((int)HttpStatusCode.Forbidden);

        var key = $"{BasePrefix(movieId)}/{quality}/{file}";
        await ProxyObjectAsync(ctx, key, "video/mp2t", TimeSpan.FromMinutes(10), ct);
        
        var (shouldCount, _) = counter.NoteSegment(movieId, file, ctx);
        if (shouldCount)
        {
            await uow.Movies.UpdateViewAsync(movieId, ct: ct);
            
            await uow.CommitAsync(ct);
        }
        
        return Results.Empty;
    }

    public async Task<IResult> GetMovieSubIndexAsync(int movieId, string lang, HttpContext ctx, CancellationToken ct)
    {
        var vip = ctx.User?.Claims?.FirstOrDefault(c => c.Type == "vip")?.Value;
        if (!await access.CanWatchMovieAsync(movieId, vip, ct))
            return Results.StatusCode((int)HttpStatusCode.Forbidden);

        var key = $"{BasePrefix(movieId)}/sub/{lang}/index.m3u8";
        await ProxyObjectAsync(ctx, key, "application/vnd.apple.mpegurl", TimeSpan.FromMinutes(5), ct);
        return Results.Empty;
    }

    public async Task<IResult> GetMovieSubSegAsync(int movieId, string lang, string file, HttpContext ctx, CancellationToken ct)
    {
        var vip = ctx.User?.Claims?.FirstOrDefault(c => c.Type == "vip")?.Value;
        if (!await access.CanWatchMovieAsync(movieId, vip, ct))
            return Results.StatusCode((int)HttpStatusCode.Forbidden);

        var key = $"{BasePrefix(movieId)}/sub/{lang}/{file}";
        await ProxyObjectAsync(ctx, key, "text/vtt; charset=utf-8", TimeSpan.FromMinutes(5), ct);
        return Results.Empty;
    }
    
     public async Task<IResult> GetEpisodeMasterAsync(int movieId, int episodeId, HttpRequest req, CancellationToken ct)
    {
        var vip = req.HttpContext.User?.Claims?.FirstOrDefault(c => c.Type == "vip")?.Value;
        if (!await access.CanWatchEpisodeAsync(movieId, episodeId, vip, ct))
            return Results.StatusCode((int)HttpStatusCode.Forbidden);
        
        var ep = await uow.Episodes.FindAsync(episodeId, ct);

        var key = $"{BasePrefix(movieId, ep!.Season!.Number, ep.Number)}/master.m3u8";
        var text = await DownloadTextAsync(key, ct);
        var rewritten = RewriteMaster(text, req, movieId, episodeId);
        return Results.Text(rewritten, "application/vnd.apple.mpegurl", Encoding.UTF8);
    }

    public async Task<IResult> GetEpisodeVariantAsync(int movieId, int episodeId, string quality, HttpContext ctx, CancellationToken ct)
    {
        Console.Write("ABC");
        var vip = ctx.User?.Claims?.FirstOrDefault(c => c.Type == "vip")?.Value;
        if (!await access.CanWatchEpisodeAsync(movieId, episodeId, vip, ct))
            return Results.StatusCode((int)HttpStatusCode.Forbidden);
        
        var ep = await uow.Episodes.FindAsync(episodeId, ct);

        var key = $"{BasePrefix(movieId, ep!.Season!.Number, ep.Number)}/{quality}/index.m3u8";
        await ProxyObjectAsync(ctx, key, "application/vnd.apple.mpegurl", TimeSpan.FromMinutes(10), ct);
        return Results.Empty;
    }

    public async Task<IResult> GetEpisodeSegmentAsync(int movieId, int episodeId, string quality, string file, HttpContext ctx, CancellationToken ct)
    {
        var vip = ctx.User?.Claims?.FirstOrDefault(c => c.Type == "vip")?.Value;
        if (!await access.CanWatchEpisodeAsync(movieId, episodeId, vip, ct))
            return Results.StatusCode((int)HttpStatusCode.Forbidden);
        
        var ep = await uow.Episodes.FindAsync(episodeId, ct);

        var key = $"{BasePrefix(movieId, ep!.Season!.Number, ep.Number)}/{quality}/{file}";
        await ProxyObjectAsync(ctx, key, "video/mp2t", TimeSpan.FromMinutes(10), ct);
        return Results.Empty;
    }

    public async Task<IResult> GetEpisodeSubIndexAsync(int movieId, int episodeId, string lang, HttpContext ctx, CancellationToken ct)
    {
        var vip = ctx.User?.Claims?.FirstOrDefault(c => c.Type == "vip")?.Value;
        if (!await access.CanWatchEpisodeAsync(movieId, episodeId, vip, ct))
            return Results.StatusCode((int)HttpStatusCode.Forbidden);
        
        var ep = await uow.Episodes.FindAsync(episodeId, ct);

        var key = $"{BasePrefix(movieId, ep!.Season!.Number, ep.Number)}/sub/{lang}/index.m3u8";
        await ProxyObjectAsync(ctx, key, "application/vnd.apple.mpegurl", TimeSpan.FromMinutes(5), ct);
        return Results.Empty;
    }

    public async Task<IResult> GetEpisodeSubSegAsync(int movieId, int episodeId, string lang, string file, HttpContext ctx, CancellationToken ct)
    {
        var vip = ctx.User?.Claims?.FirstOrDefault(c => c.Type == "vip")?.Value;
        if (!await access.CanWatchEpisodeAsync(movieId, episodeId, vip, ct))
            return Results.StatusCode((int)HttpStatusCode.Forbidden);
        
        var ep = await uow.Episodes.FindAsync(episodeId, ct);

        var key = $"{BasePrefix(movieId, ep!.Season!.Number, ep.Number)}/sub/{lang}/{file}";
        await ProxyObjectAsync(ctx, key, "text/vtt; charset=utf-8", TimeSpan.FromMinutes(5), ct);
        return Results.Empty;
    }
    
     private async Task ProxyObjectAsync(HttpContext ctx, string key, string contentType, TimeSpan ttl, CancellationToken ct)
    {
        var client = http.CreateClient("origin");
        var url = await storage.GetReadSignedUrlAsync(Bucket, key, ttl, 0, ct);
        var req = new HttpRequestMessage(HttpMethod.Get, url);

        if (ctx.Request.Headers.TryGetValue("Range", out var range))
            req.Headers.TryAddWithoutValidation("Range", (string)range!);

        using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
        ctx.Response.StatusCode = (int)resp.StatusCode;

        // Copy headers
        foreach (var h in resp.Headers)
            ctx.Response.Headers[h.Key] = new Microsoft.Extensions.Primitives.StringValues(h.Value.ToArray());
        foreach (var h in resp.Content.Headers)
            ctx.Response.Headers[h.Key] = new Microsoft.Extensions.Primitives.StringValues(h.Value.ToArray());

        // Force type & caching
        ctx.Response.ContentType = contentType;
        if (contentType == "application/vnd.apple.mpegurl")
            ctx.Response.Headers["Cache-Control"] = "public,max-age=30"; // playlist cache ngắn
        else
            ctx.Response.Headers["Cache-Control"] = "public,max-age=604800"; // 7 ngày cho .ts/.vtt

        ctx.Response.Headers.Remove("transfer-encoding");
        await resp.Content.CopyToAsync(ctx.Response.Body, ct);
    }

    private async Task<string> DownloadTextAsync(string key, CancellationToken ct)
    {
        var client = http.CreateClient("origin");
        var url = await storage.GetReadSignedUrlAsync(Bucket, key, TimeSpan.FromMinutes(5), 0, ct);
        return await client.GetStringAsync(url, ct);
    }

    private static string RewriteMaster(string master, HttpRequest req, int movieId, int? episodeId)
    {
        var apiBase = episodeId is null
            ? $"/api/stream/movies/{movieId}"
            : $"/api/stream/movies/{movieId}/episodes/{episodeId}";
        
        var allowed = new[] { 480, 720, 1080 };
        
        static int ClosestAllowed(int h, int[] allowedHeights)
            => allowedHeights.OrderBy(a => Math.Abs(a - h)).First();
        
        var lines = master.Replace("\r\n", "\n").Split('\n');

        int? pendingHeight = null;
        
        for (int i = 0; i < lines.Length; i++)
        {
            var l = lines[i].Trim();
            
            if (l.StartsWith("#EXT-X-MEDIA:", StringComparison.OrdinalIgnoreCase))
            {
                lines[i] = Regex.Replace(
                    lines[i],
                    "URI=\"([^\"]+)\"",
                    m =>
                    {
                        var old = m.Groups[1].Value;
                        if (string.IsNullOrWhiteSpace(old)) return m.Value;
                        if (old.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return m.Value;
                        if (old.StartsWith("/")) return m.Value;
                        var newUri = $"{apiBase}/{old}".Replace("//", "/");
                        return $"URI=\"{newUri}\"";
                    },
                    RegexOptions.IgnoreCase
                );
                continue;
            }

            // Dòng URI ngay sau STREAM-INF:
            //    - Chỉ rewrite nếu là relative (không bắt đầu bằng http hoặc /)
            //    - Ghi đè thành /api/stream/.../{480|720|1080}/index.m3u8
            if (!string.IsNullOrWhiteSpace(l) && !l.StartsWith("#"))
            {
                var prev = i > 0 ? lines[i - 1].TrimStart() : string.Empty;
                var isAfterStreamInf = prev.StartsWith("#EXT-X-STREAM-INF", StringComparison.OrdinalIgnoreCase);

                bool isRelative = !(l.StartsWith("http", StringComparison.OrdinalIgnoreCase) || l.StartsWith("/"));
                if (isAfterStreamInf && isRelative)
                {
                    var q = pendingHeight ?? 720; // fallback mặc định
                    // Nếu lỡ đọc ra height lạ → ép về allowed gần nhất
                    q = allowed.Contains(q) ? q : ClosestAllowed(q, allowed);

                    lines[i] = $"{apiBase}/{q}/index.m3u8";
                    pendingHeight = null;
                    continue;
                }

                // Trường hợp master có sẵn "xxx/index.m3u8" (relative) mà không đi kèm STREAM-INF (hiếm)
                // -> Nếu xxx là 480/720/1080 thì vẫn rewrite; nếu không thì map về 720.
                var m2 = Regex.Match(l, @"^(?<q>[^/]+)/index\.m3u8$", RegexOptions.IgnoreCase);
                if (isRelative && m2.Success)
                {
                    var qStr = m2.Groups["q"].Value;
                    int q;
                    if (int.TryParse(qStr, out var parsed) && allowed.Contains(parsed))
                        q = parsed;
                    else
                        q = 720;

                    lines[i] = $"{apiBase}/{q}/index.m3u8";
                    continue;
                }
            }
        }
        return string.Join("\n", lines);
    }
    private static string AppendQueryToSegments(string m3u8, string query)
    {
        var sb = new StringBuilder();
        foreach (var raw in m3u8.Replace("\r\n", "\n").Split('\n'))
        {
            var line = raw.TrimEnd();
            if (line.Length > 0 && !line.StartsWith("#") &&
                !line.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
                !line.StartsWith("/", StringComparison.Ordinal))
            {
                line = line.Contains('?') ? $"{line}&{query}" : $"{line}?{query}";
            }
            sb.AppendLine(line);
        }
        return sb.ToString();
    }
    
}