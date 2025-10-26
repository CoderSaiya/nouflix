using Microsoft.AspNetCore.Components;
using MoviePortal.Api;
using MoviePortal.Models.DTOs;

namespace MoviePortal.Components.Pages;

public class GenreEditPage : ComponentBase
{
    [Parameter] public int? Id { get; set; }
    [Inject] private TaxonomyApi TaxoApi { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;

    protected bool Loading = true;
    protected MovieDto.Genre? Model;

    protected override async Task OnInitializedAsync()
    {
        Model = Id is null ?
            new MovieDto.Genre { Id = 0, Name = "", Icon = ""} :
            await TaxoApi.GetGenreAsync(Id.Value);
        
        Loading = false;
    }

    protected async Task SaveAsync(MovieDto.Genre g)
    {
        await TaxoApi.SaveGenreAsync(g);
    }

    protected void GoBack() => Nav.NavigateTo("/genres");
}