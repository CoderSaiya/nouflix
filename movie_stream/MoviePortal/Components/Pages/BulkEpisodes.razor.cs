using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MoviePortal.Models.DTOs;
using MoviePortal.Models.Entities;
using MoviePortal.Models.ValueObject;
using MoviePortal.Models.Views;
using MoviePortal.Services;

namespace MoviePortal.Components.Pages;

public class BulkEpisodesPage() : ComponentBase
{
    [Inject] private BulkEpisodesService Bulk { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;
    
    [Parameter, SupplyParameterFromQuery(Name = "search")]
    public string? Search { get; set; }

    // View state
    protected List<(int Id, string Title, MovieType Type)> MovieOptions = new();
    protected Movie? SelectedMovie;
    protected int CurrentEpisodeCount;
    protected int MaxEpisodeNumber;
    protected int SuggestedStart => Math.Max(1, MaxEpisodeNumber + 1);

    protected readonly BulkForm Form = new();
    protected bool CanCreate => Form.MovieId is not null && Form.Count > 0 && Form.StartNumber > 0;

    protected List<PlanRow>? Preview;
    protected string? Result;

    // Actions (map 1-1 với UI cũ)
    protected async Task SearchMoviesAsync()
    {
        MovieOptions = await Bulk.SearchMoviesAsync(Search);
        if (Form.MovieId is not null && MovieOptions.All(x => x.Id != Form.MovieId))
            Form.MovieId = null;

        await LoadSelectedMovieInfo();
        StateHasChanged();
    }

    protected async Task OnMovieChanged(ChangeEventArgs _)
        => await LoadSelectedMovieInfo();

    private async Task LoadSelectedMovieInfo()
    {
        Preview = null; Result = null;
        CurrentEpisodeCount = 0; MaxEpisodeNumber = 0; SelectedMovie = null;

        if (Form.MovieId is null) return;

        var info = await Bulk.LoadMovieInfoAsync(Form.MovieId.Value);
        if (info is null) return;

        SelectedMovie = info.Value.movie;
        CurrentEpisodeCount = info.Value.count;
        MaxEpisodeNumber = info.Value.maxNumber;

        if (Form.StartNumber <= 0 || Form.StartNumber == 1)
            Form.StartNumber = SuggestedStart;
    }

    protected void SetMode(bool onlyMissing, bool overwrite)
    { Form.OnlyCreateMissing = onlyMissing; Form.OverwriteExisting = overwrite; }

    protected async Task BuildPreview()
    {
        Preview = null; Result = null;
        if (!CanCreate || Form.MovieId is null) return;

        Preview = await Bulk.BuildPlanAsync(
            Form.MovieId.Value, Form.StartNumber, Form.Count,
            Form.TitlePattern ?? "Tập {n}",
            Form.ReleaseStartDate, Form.ReleaseIntervalDays);
    }

    protected async Task CreateAsync()
    {
        if (Preview is null) await BuildPreview();
        if (Form.MovieId is null || Preview is null) return;

        var confirm = await Js.InvokeAsync<bool>("confirm", "Xác nhận tạo/cập nhật các tập theo danh sách xem trước?");
        if (!confirm) return;

        var (created, updated, skipped) = await Bulk.CreateAsync(
            Form.MovieId.Value, Preview, Form.OverwriteExisting,
            Form.Synopsis, Form.DurationMinutes,
            Form.Status, Form.Quality, Form.IsVipOnly);

        Result = $"Tạo mới: {created}, Cập nhật: {updated}, Bỏ qua: {skipped}.";
        await LoadSelectedMovieInfo();
    }
}