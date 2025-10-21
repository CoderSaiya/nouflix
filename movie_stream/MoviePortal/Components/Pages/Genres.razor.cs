using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MoviePortal.Api;
using MoviePortal.Models.DTOs;

namespace MoviePortal.Components.Pages;

public class GenresPage : ComponentBase
{
    [Inject] private TaxonomyApi TaxoApi { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;

    protected string? Q;
    protected List<MovieDto.Genre> Items = new();

    protected override async Task OnInitializedAsync() => await LoadAsync();
    protected override async Task OnParametersSetAsync() => await LoadAsync();

    private async Task LoadAsync()
        => Items = await TaxoApi.SearchGenresAsync(Q);

    protected void NewGenre() => Nav.NavigateTo("/genres/edit");
    protected void EditGenre(int id) => Nav.NavigateTo($"/genres/edit/{id}");

    protected async Task DeleteAsync(int id)
    {
        if (!await Js.InvokeAsync<bool>("confirm", "Xoá thể loại này?")) return;
        
        await TaxoApi.DeleteGenreAsync(id);
        await LoadAsync();
        
        StateHasChanged();
    }
}