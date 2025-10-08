using Microsoft.AspNetCore.Components;
using MoviePortal.Models.Entities;
using MoviePortal.Models.ValueObject;
using MoviePortal.Services;

namespace MoviePortal.Components.Pages;

public class MovieEditPage : ComponentBase
{
    [Parameter] public int? Id { get; set; }

    [Inject] private MoviesService Movies { get; set; } = null!;
    [Inject] private TaxonomyService Taxo { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;

    protected bool Loading = true;
    protected Movie? Movie;
    protected string Tab = "info";
    protected bool IsCreate => !Id.HasValue || Id.Value == 0;

    protected List<Genre> AllGenres = new();
    protected HashSet<int> SelectedGenreIds = new();
    protected List<Studio> AllStudios = new();
    protected HashSet<int> SelectedStudioIds = new();

    protected override async Task OnInitializedAsync()
    {
        AllGenres = await Taxo.SearchGenresAsync(null);
        AllStudios = await Taxo.SearchStudiosAsync(null);

        if (IsCreate)
        {
            Movie = new Movie
            {
                Title = "",
                Status = PublishStatus.Draft,
                Type = MovieType.Single,
                Language = "vi",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            Loading = false;
            return;
        }

        Movie = await Movies.GetAsync(Id!.Value);
        if (Movie is null) { Loading = false; return; }

        SelectedGenreIds  = Movie.MovieGenres.Select(x => x.GenreId).ToHashSet();
        SelectedStudioIds = Movie.MovieStudios.Select(x => x.StudioId).ToHashSet();
        Loading = false;
    }

    protected async Task SaveAsync()
    {
        if (Movie is null) return;

        if (IsCreate || Movie.Id == 0)
        {
            var newId = await Movies.CreateAsync(Movie, SelectedGenreIds, SelectedStudioIds);
            Nav.NavigateTo($"/movies/edit/{newId}", forceLoad: true);
        }
        else
        {
            await Movies.UpdateAsync(Movie, SelectedGenreIds, SelectedStudioIds);
        }
    }

    protected void GoBack() => Nav.NavigateTo("/movies");
}