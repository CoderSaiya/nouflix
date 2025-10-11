using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace NouFlix.Adapters;

public sealed class ViewCounter
{
    private sealed class State
    {
        public int Segments;
        public string? LastSeg;
        public bool Qualified;
        public DateTime LastTouch = DateTime.UtcNow;
    }

    private readonly ConcurrentDictionary<string, State> _mem = new();
    private static readonly TimeSpan SessionTtl = TimeSpan.FromMinutes(20);
    private const int SegmentsThreshold = 2; 
    
    public (bool ShouldCount, int Segments) NoteSegment(int movieId, string segFile, HttpContext ctx)
    {
        var ip = GetClientIp(ctx);
        var ua = ctx.Request.Headers.UserAgent.ToString();
        if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(ua))
            return (false, 0);

        var uaHash = HashShort(ua);
        var key = $"m:{movieId}:ip:{ip}:ua:{uaHash}";

        var name = Path.GetFileName(segFile);
        var st = _mem.AddOrUpdate(key,
            _ => new State { Segments = 1, LastSeg = name },
            (_, s) =>
            {
                if (!string.Equals(s.LastSeg, name, StringComparison.OrdinalIgnoreCase))
                {
                    s.Segments++;
                    s.LastSeg = name;
                }
                s.LastTouch = DateTime.UtcNow;
                return s;
            });

        // dọn rác
        if (DateTime.UtcNow - st.LastTouch > SessionTtl)
            _mem.TryRemove(key, out _);

        if (!st.Qualified && st.Segments >= SegmentsThreshold)
        {
            st.Qualified = true; // idempotent 1 lần/ key
            return (true, st.Segments);
        }

        return (false, st.Segments);
    }

    private static string? GetClientIp(HttpContext ctx)
    {
        // ưu tiên header nếu có reverse proxy / CDN
        if (ctx.Request.Headers.TryGetValue("CF-Connecting-IP", out var cf)) return cf.ToString();
        if (ctx.Request.Headers.TryGetValue("X-Forwarded-For", out var xff))
            return xff.ToString().Split(',')[0].Trim();
        return ctx.Connection.RemoteIpAddress?.ToString();
    }

    private static string HashShort(string s)
    {
        using var sha1 = SHA1.Create();
        var b = sha1.ComputeHash(Encoding.UTF8.GetBytes(s));
        return Convert.ToHexString(b.AsSpan(0, 6)); // 12 hex chars
    }
}