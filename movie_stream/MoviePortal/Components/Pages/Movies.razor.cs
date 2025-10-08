using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using MoviePortal.Models.Entities;
using MoviePortal.Services;

namespace MoviePortal.Components.Pages;

public class MoviesPage : ComponentBase
{
    [Inject] private MoviesService Movies { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;

    protected string? Search;

    protected void PreviewMovie(int id) => Nav.NavigateTo($"movies/view/{id}");
    protected void NewMovie() => Nav.NavigateTo("/movies/edit");
    protected void EditMovie(int id) => Nav.NavigateTo($"/movies/edit/{id}");

    protected async Task DeleteAsync(int id)
    {
        if (!await Js.InvokeAsync<bool>("confirm", "Bạn chắc chắn xoá?")) return;
        var mov = await Movies.GetAsync(id);
        if (mov != null) await Movies.DeleteAsync(mov);
        StateHasChanged(); // Virtualize sẽ gọi lại ItemsProvider ở lần vẽ sau
    }

    protected async ValueTask<ItemsProviderResult<Movie>> LoadMovies(ItemsProviderRequest req)
    {
        var (total, items) = await Movies.SearchAsync(
            Search,
            req.StartIndex,
            req.Count,
            req.CancellationToken);

        return new ItemsProviderResult<Movie>(items, total);
    }
}