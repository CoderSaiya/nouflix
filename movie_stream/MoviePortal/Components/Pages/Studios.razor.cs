using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MoviePortal.Api;
using MoviePortal.Models.DTOs;

namespace MoviePortal.Components.Pages;

public class StudiosPage : ComponentBase
{
    [Inject] private TaxonomyApi TaxoApi { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;

    protected string? Q;
    protected List<MovieDto.Studio> Items = new();

    protected override async Task OnInitializedAsync() => await LoadAsync();
    protected override async Task OnParametersSetAsync() => await LoadAsync();

    private async Task LoadAsync()
        => Items = await TaxoApi.SearchStudiosAsync(Q);

    protected void NewStudio() => Nav.NavigateTo("/studios/edit");
    protected void EditStudio(int id) => Nav.NavigateTo($"/studios/edit/{id}");

    protected async Task DeleteAsync(int id)
    {
        if (!await Js.InvokeAsync<bool>("confirm", "Xoá studio này?")) return;
        
        await TaxoApi.DeleteStudioAsync(id);
        await LoadAsync();
        
        StateHasChanged();
    }
}