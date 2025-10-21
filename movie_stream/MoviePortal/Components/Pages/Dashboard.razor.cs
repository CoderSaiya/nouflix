using Microsoft.AspNetCore.Components;
using MoviePortal.Api;
using MoviePortal.Models.DTOs;

namespace MoviePortal.Components.Pages;

public class DashboardPage : ComponentBase
{
    [Inject] private DashboardApi Api { get; set; } = null!;

    protected bool Loading = true;

    protected SystemDto.Kpis Kpis = null!;
    protected SystemDto.TaxonomyCounts Taxo = null!;
    protected List<MovieDto.Movie> RecentMovies = new(); 
    protected List<MovieDto.Movie> TopByViews = new();
    protected List<SystemDto.IssueItem> Issues = new();
    protected List<AssetsDto.Image> OrphanSamples = new();

    protected override async Task OnInitializedAsync()
    {
        var res = await Api.GetAsync();
        if (res is null) return;

        (Kpis, Taxo, RecentMovies, TopByViews, Issues, OrphanSamples) = res;
        
        Loading = false;
    }

    protected static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        if (bytes == 0) return "0 B";
        var i = (int)Math.Floor(Math.Log(bytes, 1024));
        var v = bytes / Math.Pow(1024, i);
        return $"{v:0.##} {sizes[i]}";
    }
}