using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NouFlix.DTOs;
using NouFlix.Mapper;
using NouFlix.Models.Common;
using NouFlix.Services;

namespace NouFlix.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(MinioObjectStorage storage, DashboardService svc) : Controller
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ResponseCache(Duration = 5, Location = ResponseCacheLocation.Client, NoStore = false)]
    public async Task<IActionResult> Get(CancellationToken ct = default)
    {
        var (kpis, taxo, recent, topViews, issues, orphanImages) = await svc.BuildAsync(ct);
        
        var orphanDtos = await Task.WhenAll(orphanImages.Select(i => i.ToImageAssetResAsync(storage, ct)));

        var res = new SystemDto.DashboardRes(
            Kpis: kpis,
            Taxonomy: taxo,
            RecentMovies: recent,
            TopViewedMovies: topViews,
            Issues: issues,
            OrphanImages: orphanDtos.ToList()
        );

        return Ok(GlobalResponse<SystemDto.DashboardRes>.Success(res));
    }
}