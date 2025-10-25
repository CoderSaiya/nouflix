using NouFlix.DTOs;
using NouFlix.Models.Entities;

namespace NouFlix.Mapper;

public static class GenreMapper
{
    public static Task<GenreDto.GenreRes> ToGenreResAsync(
        this Genre g,
        CancellationToken ct = default)
        => Task.FromResult(new GenreDto.GenreRes(g.Id, g.Name, g.Icon, g.MovieCount));

    public static Task<GenreDto.GenreRes[]> ToGenreResListAsync(
        this IEnumerable<Genre> genres,
        CancellationToken ct = default)
        => Task.WhenAll(genres.Select(g => ToGenreResAsync(g, ct)).ToArray());
}