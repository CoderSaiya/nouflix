using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using NouFlix.DTOs;

namespace NouFlix.Adapters;

public class AspNetExternalTicketReader
{
    public async Task<ExternalTicket?> ReadAsync(HttpContext ctx, string externalScheme)
    {
        var result = await ctx.AuthenticateAsync(externalScheme);
        if (!result.Succeeded) return null;

        var p = result.Ticket!.AuthenticationScheme; // "Google"/"GitHub"
        var principal = result.Principal!;
        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        var key = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        return new ExternalTicket(p, key!, email, principal.Claims);
    }
}