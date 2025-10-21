using System.Globalization;
using Microsoft.AspNetCore.Components;
using MoviePortal.Api;
using MoviePortal.Models.DTOs;
using MoviePortal.Models.ValueObject;

namespace MoviePortal.Components.Pages;

public class MovieEditPage : ComponentBase
{
    [Parameter] public int? Id { get; set; }
    [Inject] private MovieApi MovApi { get; set; } = null!;
    [Inject] private TaxonomyApi TaxoApi { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;

    protected bool Loading = true;
    protected MovieDto.Movie? Movie;
    protected string Tab = "info";
    protected bool IsCreate => !Id.HasValue || Id.Value == 0;

    protected List<MovieDto.Genre> AllGenres = new();
    protected HashSet<int> SelectedGenreIds = new();
    protected List<MovieDto.Studio> AllStudios = new();
    protected HashSet<int> SelectedStudioIds = new();

    protected override async Task OnInitializedAsync()
    {
        AllGenres = await TaxoApi.SearchGenresAsync(null);
        AllStudios = await TaxoApi.SearchStudiosAsync(null);

        if (IsCreate)
        {
            Movie = new MovieDto.Movie
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

        Movie = await MovApi.GetAsync(Id!.Value);
        if (Movie is null) { Loading = false; return; }

        SelectedGenreIds = Movie.Genres.Select(x => x.Id).ToHashSet();
        SelectedStudioIds = Movie.Studios.Select(x => x.Id).ToHashSet();
        Loading = false;
    }

    protected async Task SaveAsync()
    {
        if (Movie is null) return;

        var req = new MovieDto.UpsertMovieReq(
            Movie.Title,
            Movie.AlternateTitle,
            Movie.Slug,
            Movie.Overview,
            Movie.Director,
            Movie.Country,
            Movie.Language,
            Movie.AgeRating,
            Movie.ReleaseDate,
            Movie.Type,
            Movie.Status,
            Movie.Quality,
            Movie.IsVipOnly,
            SelectedGenreIds,
            SelectedStudioIds
        );

        if (IsCreate || Movie.Id == 0)
        {
            var newId = await MovApi.SaveAsync(req, 0);
            Nav.NavigateTo($"/movies/edit/{newId}", forceLoad: true);
        }
        else
        {
            await MovApi.SaveAsync(req, Movie.Id);
            Nav.NavigateTo($"/movies/edit/{Movie.Id}", forceLoad: true);
        }
    }

    protected void GoBack() => Nav.NavigateTo("/movies");
}