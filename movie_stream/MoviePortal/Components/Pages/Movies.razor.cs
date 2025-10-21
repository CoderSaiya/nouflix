using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using MoviePortal.Api;
using MoviePortal.Models.DTOs;

namespace MoviePortal.Components.Pages;

public class MoviesPage : ComponentBase
{
    [Inject] private NavigationManager Nav { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;
    [Inject] private MovieApi MovieApi { get; set; } = null!;

    protected string? Search;

    protected void PreviewMovie(int id) => Nav.NavigateTo($"/movies/view/{id}");
    protected void NewMovie() => Nav.NavigateTo("/movies/edit");
    protected void EditMovie(int id) => Nav.NavigateTo($"/movies/edit/{id}");

    protected async Task DeleteAsync(int id)
    {
        if (!await Js.InvokeAsync<bool>("confirm", "Bạn chắc chắn xoá?")) return;
        
        await MovieApi.DeleteAsync(id);
        
        StateHasChanged(); // Virtualize sẽ gọi lại ItemsProvider ở lần vẽ sau
    }

    protected async ValueTask<ItemsProviderResult<MovieDto.MovieSummary>> LoadMovies(ItemsProviderRequest req)
    {
        var (total, items) = await MovieApi.SearchAsync(
            Search,
            req.StartIndex,
            req.Count,
            req.CancellationToken);

        return new ItemsProviderResult<MovieDto.MovieSummary>(items, total ?? 0);
    }
}