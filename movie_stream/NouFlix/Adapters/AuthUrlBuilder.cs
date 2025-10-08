using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using NouFlix.Models.Specification;

namespace NouFlix.Adapters;

public class AuthUrlBuilder(IOptions<AuthOptions> opt)
{
    private readonly AuthOptions _opt = opt.Value;
    private static readonly JsonSerializerOptions JsonOpt = new(JsonSerializerDefaults.Web);

    public string NormalizeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return Combine(_opt.FrontendBaseUrl, _opt.SuccessPath);

        if (Uri.TryCreate(returnUrl, UriKind.Absolute, out var abs))
        {
            if (_opt.AllowedReturnUrlHosts.Length > 0 &&
                !_opt.AllowedReturnUrlHosts.Contains(abs.Host, StringComparer.OrdinalIgnoreCase))
                return Combine(_opt.FrontendBaseUrl, _opt.SuccessPath);
            return abs.ToString();
        }

        // relative path → gắn vào FE
        if (returnUrl.StartsWith("/")) return Combine(_opt.FrontendBaseUrl, returnUrl);
        return Combine(_opt.FrontendBaseUrl, _opt.SuccessPath);
    }

    public string LoginWithError(string error)
    {
        var url = Combine(_opt.FrontendBaseUrl, _opt.LoginPath);
        return $"{url}?error={Uri.EscapeDataString(error)}";
    }

    public string Success(string? returnUrl, string accessToken, DateTimeOffset expiresAt, object user)
    {
        var success = NormalizeReturnUrl(returnUrl);
        var payload = new {
            access_token = accessToken,
            expires_at = expiresAt.ToUnixTimeSeconds(),
            token_type = "Bearer",
            user
        };
        var json = JsonSerializer.Serialize(payload, JsonOpt);
        var b64 = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(json));
        return $"{success}#auth={b64}";
    }

    private static string Combine(string baseUrl, string path)
        => $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
}