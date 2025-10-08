using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MoviePortal.Models.Entities;
using MoviePortal.Services;

namespace MoviePortal.Components.Pages;

public class GenresPage : ComponentBase
{
    [Inject] private TaxonomyService Taxo { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;

    protected string? Q;
    protected List<Genre> Items = new();

    protected override async Task OnInitializedAsync() => await LoadAsync();
    protected override async Task OnParametersSetAsync() => await LoadAsync();

    private async Task LoadAsync()
        => Items = await Taxo.SearchGenresAsync(Q);

    protected void NewGenre() => Nav.NavigateTo("/genres/edit");
    protected void EditGenre(int id) => Nav.NavigateTo($"/genres/edit/{id}");

    protected async Task DeleteAsync(int id)
    {
        if (!await Js.InvokeAsync<bool>("confirm", "Xoá thể loại này?")) return;
        var item = Items.FirstOrDefault(e => e.Id == id);
        if (item != null) await Taxo.DeleteGenreAsync(item);
        await LoadAsync();
        StateHasChanged();
    }
}