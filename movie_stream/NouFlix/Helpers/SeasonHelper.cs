namespace NouFlix.Helpers;

public static class SeasonHelper
{
    public static string GetOrCreateSid(HttpContext ctx, HttpResponse res)
    {
        if (ctx.Request.Cookies.TryGetValue("sid", out var sid) && !string.IsNullOrWhiteSpace(sid))
            return sid;

        sid = Guid.NewGuid().ToString("N");
        res.Cookies.Append("sid", sid, new CookieOptions {
            HttpOnly = true,
            Secure = ctx.Request.IsHttps,
            SameSite = SameSiteMode.Lax, // ổn khi cùng-site; cross-site thì ta đã gắn vào query rồi
            Path = "/api/stream",
            MaxAge = TimeSpan.FromHours(2)
        });
        return sid;
    }
}