using Microsoft.AspNetCore.Components;
using MoviePortal.Models.Entities;
using MoviePortal.Services;

namespace MoviePortal.Components.Pages;

public class StudioEditPage : ComponentBase
{
    [Parameter] public int? Id { get; set; }
    [Inject] private TaxonomyService Taxo { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;

    protected bool Loading = true;
    protected Studio? Model;

    protected override async Task OnInitializedAsync()
    {
        Model = Id is null ? new Studio { Name = "" } : await Taxo.GetStudioAsync(Id.Value);
        Loading = false;
    }

    protected async Task SaveAsync(Studio s)
    {
        await Taxo.SaveStudioAsync(s);
    }

    protected void GoBack() => Nav.NavigateTo("/studios");
}