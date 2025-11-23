using System.Net;
using System.Security.Claims;
using NouFlix.DTOs;
using NouFlix.Models.Common;
using Serilog;

namespace NouFlix.Services;

public class ExternalAuth(
    UserService users,
    AuthService auth,
    IConfiguration cfg,
    IHttpContextAccessor accessor)
{
    private static readonly HashSet<string> Allowed = ["google","facebook"];
    
    private readonly Serilog.ILogger _logger = Log.ForContext<ExternalAuth>();
    private HttpContext? HttpContext => accessor.HttpContext;

    private string? ClientIp => HttpContext?.Connection.RemoteIpAddress?.ToString();
    private string? UserAgent => HttpContext?.Request.Headers["User-Agent"].ToString();

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
        {
            var failAudit = new SystemDto.AuditLog
            {
                Id = Guid.NewGuid().ToString(),
                CorrelationId = (string?)HttpContext?.Request.Headers["X-Correlation-Id"] ?? HttpContext?.TraceIdentifier,
                UserId = null,
                Username = null,
                Action = "login",
                ResourceType = "User",
                ResourceId = t.ProviderKey,
                Details = $"ExternalLoginFailed provider={t.Provider} reason=missing_provider_key",
                ClientIp = ClientIp,
                UserAgent = UserAgent,
                Route = HttpContext?.Request.Path.ToString(),
                HttpMethod = HttpContext?.Request.Method,
                StatusCode = StatusCodes.Status400BadRequest,
            };
            _logger.Information("Auth audit {@Audit}", failAudit);
            
            return new(false, "missing_provider_key");
        }
            

        var user = await users.FindOrCreateExternal(t.Provider, t.ProviderKey, t.Email, t.Avatar, new ClaimsPrincipal(new ClaimsIdentity(t.Claims)));
        var issued = await auth.IssueTokensForUserAsync(user);
        var userDto = await auth.GetCurrentUserAsync(user.Id);
        
        var successAudit = new SystemDto.AuditLog
        {
            Id = Guid.NewGuid().ToString(),
            CorrelationId = (string?)HttpContext?.Request.Headers["X-Correlation-Id"] ?? HttpContext?.TraceIdentifier,
            UserId = user.Id.ToString(),
            Username = user.Email.ToString(),
            Action = "login",
            ResourceType = "User",
            ResourceId = user.Id.ToString(),
            Details = $"ExternalLoginSuccess provider={t.Provider}",
            ClientIp = ClientIp,
            UserAgent = UserAgent,
            Route = HttpContext?.Request.Path.ToString(),
            HttpMethod = HttpContext?.Request.Method,
            StatusCode = StatusCodes.Status200OK,
        };

        _logger.Information("Auth audit {@Audit}", successAudit);

        return new(true, null, issued.AccessToken, issued.AccessExpiresAt, issued.RefreshToken, userDto);
    }
}