using Microsoft.AspNetCore.Components;
using MoviePortal.Api;
using MoviePortal.Models.DTOs;

namespace MoviePortal.Components.Pages;

public class StudioEditPage : ComponentBase
{
    [Parameter] public int? Id { get; set; }
    [Inject] private TaxonomyApi TaxoApi { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;

    protected bool Loading = true;
    protected MovieDto.Studio? Model;

    protected override async Task OnInitializedAsync()
    {
        Model = Id is null ?
            new MovieDto.Studio { Name = "" } :
            await TaxoApi.GetStudioAsync(Id.Value);
        
        Loading = false;
    }

    protected async Task SaveAsync(MovieDto.Studio s)
    {
        await TaxoApi.SaveStudioAsync(s);
    }

    protected void GoBack() => Nav.NavigateTo("/studios");
}