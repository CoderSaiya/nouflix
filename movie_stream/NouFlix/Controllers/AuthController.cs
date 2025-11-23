using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NouFlix.Adapters;
using NouFlix.DTOs;
using NouFlix.Models.Common;
using NouFlix.Services;

namespace NouFlix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    AuthService svc,
    ExternalAuth external,
    AuthUrlBuilder url,
    TokenCookieWriter cookie,
    AspNetExternalTicketReader tr
) : Controller
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(RegisterReq req)
    {
        await svc.RegisterAsync(req);
        return Created($"/api/auth/users/{Uri.EscapeDataString(req.Email)}",
            GlobalResponse<string>.Success("Đăng ký thành công"));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromForm] LoginReq req)
    {
        var res = await svc.LoginAsync(req);

        cookie.WriteRefresh(Response, res.RefreshToken);

        return Ok(GlobalResponse<AuthRes>.Success(res));
    }
    
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        cookie.DeleteRefresh(Response);

        svc.LogoutAsync();

        return Ok(GlobalResponse<string>.Success("Đã đăng xuất"));
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(CancellationToken ct)
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(
                GlobalResponse<string>.Error("Missing refresh token.", StatusCodes.Status401Unauthorized)
            );
        }
        
        var newAccessToken = await svc.RefreshTokenAsync(refreshToken, ct);
        if (newAccessToken is null)
        {
            return Unauthorized(
                GlobalResponse<string>.Error("Refresh token expired or invalid.", StatusCodes.Status401Unauthorized)
            );
        }
        
        return Ok(
            GlobalResponse<object>.Success(new { AccessToken = newAccessToken })
        );
    }
    
    [HttpGet("external/{provider}/start")]
    [AllowAnonymous]
    public IActionResult ExternalStart(string provider, [FromQuery] string? returnUrl)
    {
        var (scheme, uri) = external.BuildUri(provider, returnUrl);

        // Controller tự tạo AuthenticationProperties (thuần HTTP concern)
        var props = new AuthenticationProperties { RedirectUri = uri };
        return Challenge(props, scheme);
    }

    [HttpGet("external/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalCallback([FromQuery] string? returnUrl)
    {
        var ticket = await tr.ReadAsync(HttpContext, "External");
        if (ticket is null) return Redirect(url.LoginWithError("external_failed"));

        var result = await external.SignInWithExternalAsync(ticket);
        if (!result.Succeeded) return Redirect(url.LoginWithError(result.Error!));

        cookie.WriteRefresh(Response, result.RefreshToken!);
        var successUrl = url.Success(returnUrl, result.AccessToken!, result.ExpiresAt!.Value, result.User!);
        return Redirect(successUrl);
    }
    
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr))
            return Unauthorized(GlobalResponse<string>.Error("Missing sub/NameIdentifier.", StatusCodes.Status401Unauthorized));

        var userId = Guid.Parse(userIdStr);
        var dto = await svc.GetCurrentUserAsync(userId);
        return Ok(GlobalResponse<UserDto.UserRes>.Success(dto));
    }
}