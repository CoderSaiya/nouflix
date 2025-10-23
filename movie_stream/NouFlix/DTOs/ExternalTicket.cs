using System.Security.Claims;

namespace NouFlix.DTOs;

public record ExternalTicket(
    string Provider,
    string ProviderKey,
    string? Email,
    string? Avatar,
    IEnumerable<Claim> Claims);