using Microsoft.Extensions.Options;
using NouFlix.Models.Specification;

namespace NouFlix.Adapters;

public class TokenCookieWriter(IOptions<AuthCookieOptions> opt, IHostEnvironment env)
{
    private readonly AuthCookieOptions _opt = opt.Value;

    public void WriteRefresh(HttpResponse response, string refreshToken, DateTimeOffset? expiresAt = null)
    {
        var sameSite = _opt.CrossSite ? SameSiteMode.None : SameSiteMode.Lax;
        // Cross-site thì bắt buộc Secure=true (trình duyệt mới chấp nhận SameSite=None)
        var secure = _opt.CrossSite || !env.IsDevelopment();

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = sameSite,
            Path = _opt.Path,
            Expires = (expiresAt ?? DateTimeOffset.UtcNow.AddDays(_opt.RefreshDays))
        };
        if (!string.IsNullOrWhiteSpace(_opt.Domain))
            cookieOptions.Domain = _opt.Domain;

        response.Cookies.Append(_opt.RefreshCookieName, refreshToken, cookieOptions);
    }

    public void DeleteRefresh(HttpResponse response)
    {
        var sameSite = _opt.CrossSite ? SameSiteMode.None : SameSiteMode.Lax;
        var secure = _opt.CrossSite || !env.IsDevelopment();

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = secure,
            SameSite = sameSite,
            Path = _opt.Path,
            Expires = DateTimeOffset.UnixEpoch
        };
        if (!string.IsNullOrWhiteSpace(_opt.Domain))
            cookieOptions.Domain = _opt.Domain;

        // Ghi đè cookie với giá trị rỗng + expire quá khứ để xoá trên trình duyệt
        response.Cookies.Append(_opt.RefreshCookieName, string.Empty, cookieOptions);
    }
}