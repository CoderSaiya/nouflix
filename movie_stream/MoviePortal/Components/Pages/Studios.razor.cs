using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MoviePortal.Models.Entities;
using MoviePortal.Services;

namespace MoviePortal.Components.Pages;

public class StudiosPage : ComponentBase
{
    [Inject] private TaxonomyService Taxo { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;
    [Inject] private IJSRuntime Js { get; set; } = null!;

    protected string? Q;
    protected List<Studio> Items = new();

    protected override async Task OnInitializedAsync() => await LoadAsync();
    protected override async Task OnParametersSetAsync() => await LoadAsync();

    private async Task LoadAsync()
        => Items = await Taxo.SearchStudiosAsync(Q);

    protected void NewStudio() => Nav.NavigateTo("/studios/edit");
    protected void EditStudio(int id) => Nav.NavigateTo($"/studios/edit/{id}");

    protected async Task DeleteAsync(int id)
    {
        if (!await Js.InvokeAsync<bool>("confirm", "Xoá studio này?")) return;
        var item = Items.FirstOrDefault(e => e.Id == id);
        if (item != null) await Taxo.DeleteStudioAsync(item);
        await LoadAsync();
        StateHasChanged();
    }
}