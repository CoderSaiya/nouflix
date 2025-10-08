using Microsoft.AspNetCore.Components;
using MoviePortal.Models.Entities;
using MoviePortal.Services;

namespace MoviePortal.Components.Pages;

public class GenreEditPage : ComponentBase
{
    [Parameter] public int? Id { get; set; }
    [Inject] private TaxonomyService Taxo { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    protected bool Loading = true;
    protected Genre? Model;

    protected override async Task OnInitializedAsync()
    {
        Model = Id is null ? new Genre { Name = "" } : await Taxo.GetGenreAsync(Id.Value);
        Loading = false;
    }

    protected async Task SaveAsync(Genre g)
    {
        await Taxo.SaveGenreAsync(g);
    }

    protected void GoBack() => Nav.NavigateTo("/genres");
}