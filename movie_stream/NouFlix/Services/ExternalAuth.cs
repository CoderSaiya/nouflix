using System.Net;
using System.Security.Claims;
using NouFlix.DTOs;
using NouFlix.Models.Common;

namespace NouFlix.Services;

public class ExternalAuth(UserService users, AuthService auth)
{
    private static readonly HashSet<string> Allowed = ["google","github"];  // tốt hơn: lấy từ config

    public ExternalChallenge BuildChallenge(string provider, string? returnUrl)
    {
        var p = provider.ToLowerInvariant();
        if (!Allowed.Contains(p)) throw new UnsupportedProviderException();

        var scheme = p == "google" ? "Google" : "GitHub";
        // chỉ trả path, controller sẽ gắn RedirectUri
        return new ExternalChallenge(scheme, "/api/auth/external/callback?returnUrl=" + 
                                             WebUtility.UrlEncode(returnUrl)); 
    }

    public async Task<ExternalSignInResult> SignInWithExternalAsync(ExternalTicket t)
    {
        if (string.IsNullOrEmpty(t.ProviderKey))
            return new(false, "missing_provider_key");

        var user = await users.FindOrCreateExternal(t.Provider, t.ProviderKey, t.Email, new ClaimsPrincipal(new ClaimsIdentity(t.Claims)));
        var issued = await auth.IssueTokensForUserAsync(user);
        var userDto = await auth.GetCurrentUserAsync(user.Id);

        return new(true, null, issued.AccessToken, issued.AccessExpiresAt, issued.RefreshToken, userDto);
    }
}