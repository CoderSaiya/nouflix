using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NouFlix.Services;

namespace NouFlix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogController(LogService svc) : Controller
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int size = 25,
        [FromQuery] string? q = null, [FromQuery] string? level = null,
        [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        size = Math.Min(size, 100);
        var (total, items) = await svc.SearchLogsAsync(page, size, q, level, from, to);
        return Ok(new { total, page, size, items });
    }
}