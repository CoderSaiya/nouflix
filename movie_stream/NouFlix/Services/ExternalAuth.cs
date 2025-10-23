using System.Net;
using System.Security.Claims;
using NouFlix.DTOs;
using NouFlix.Models.Common;

namespace NouFlix.Services;

public class ExternalAuth(UserService users, AuthService auth, IConfiguration cfg)
{
    private static readonly HashSet<string> Allowed = ["google","facebook"];

    public (string, string) BuildUri(string provider, string? returnUrl)
    {
        var p = provider.ToLowerInvariant();
        if (!Allowed.Contains(p)) throw new UnsupportedProviderException();

        var scheme = p == "google" ? "Google" : "Facebook";
        
        var fallbackReturn = $"{cfg["Auth:Auth:FrontendBaseUrl"]}/auth/sso/success";
        var encoded = WebUtility.UrlEncode(returnUrl ?? fallbackReturn);
        
        var redirectUri = $"/api/auth/external/callback?returnUrl={encoded}";
        
        // chỉ trả path, controller sẽ gắn RedirectUri
        return (scheme, redirectUri); 
    }

    public async Task<ExternalSignInResult> SignInWithExternalAsync(ExternalTicket t)
    {
        if (string.IsNullOrEmpty(t.ProviderKey))
            return new(false, "missing_provider_key");

        var user = await users.FindOrCreateExternal(t.Provider, t.ProviderKey, t.Email, t.Avatar, new ClaimsPrincipal(new ClaimsIdentity(t.Claims)));
        var issued = await auth.IssueTokensForUserAsync(user);
        var userDto = await auth.GetCurrentUserAsync(user.Id);

        return new(true, null, issued.AccessToken, issued.AccessExpiresAt, issued.RefreshToken, userDto);
    }
}