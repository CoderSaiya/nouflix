using Microsoft.AspNetCore.Components;
using MoviePortal.Models.DTOs;
using MoviePortal.Models.Entities;
using MoviePortal.Services;

namespace MoviePortal.Components.Pages;

public class DashboardPage : ComponentBase
{
    [Inject] private DashboardService Dash { get; set; } = null!;

    protected bool _loading = true;

    protected Kpis Kpis = null!;
    protected TaxonomyCounts Taxo = null!;
    protected List<Movie> RecentMovies = new();
    protected List<Movie> TopByViews = new();
    protected List<IssueItem> Issues = new();
    protected List<ImageAsset> OrphanSamples = new();

    protected override async Task OnInitializedAsync()
    {
        (Kpis, Taxo, RecentMovies, TopByViews, Issues, OrphanSamples) = await Dash.BuildAsync();
        _loading = false;
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