using Microsoft.AspNetCore.Components;
using MoviePortal.Models.DTOs;
using MoviePortal.Models.ValueObject;
using MoviePortal.Services;

namespace MoviePortal.Components.Pages;

public class HealthPage : ComponentBase
{
    [Inject] private SystemHealthService Health { get; set; } = null!;
    protected bool Loading = true;
    protected SystemHealthReport? Report;

    protected override async Task OnInitializedAsync()
    {
        await RunAsync();
    }

    protected async Task RunAsync()
    {
        Loading = true;
        StateHasChanged();
        Report = await Health.CheckAllAsync();
        Loading = false;
    }

    // helpers
    protected static string RowClass(HealthState s) => s switch
    {
        HealthState.Healthy => "table-success",
        HealthState.Degraded => "table-warning",
        HealthState.Unhealthy => "table-danger",
        HealthState.NotConfigured => "table-light",
        _ => ""
    };

    protected static MarkupString Badge(HealthState s)
    {
        var (cls, text) = s switch
        {
            HealthState.Healthy => ("success", "Healthy"),
            HealthState.Degraded => ("warning", "Degraded"),
            HealthState.Unhealthy => ("danger", "Unhealthy"),
            _ => ("secondary", "Not configured")
        };
        return (MarkupString)$"<span class='badge bg-{cls}'>{text}</span>";
    }

    protected static string FormatUptime(TimeSpan t)
    {
        if (t.TotalDays >= 1) return $"{(int)t.TotalDays}d {t.Hours}h {t.Minutes}m";
        if (t.TotalHours >= 1) return $"{(int)t.TotalHours}h {t.Minutes}m";
        if (t.TotalMinutes >= 1) return $"{(int)t.TotalMinutes}m {t.Seconds}s";
        return $"{t.Seconds}s";
    }
}