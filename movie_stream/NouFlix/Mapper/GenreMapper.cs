using NouFlix.DTOs;
using NouFlix.Models.Entities;

namespace NouFlix.Mapper;

public static class GenreMapper
{
    public static Task<GenreRes> ToGenreResAsync(
        this Genre g,
        CancellationToken ct = default)
        => Task.FromResult(new GenreRes(g.Id, g.Name));

    public static Task<GenreRes[]> ToGenreResListAsync(
        this IEnumerable<Genre> genres,
        CancellationToken ct = default)
        => Task.WhenAll(genres.Select(g => ToGenreResAsync(g, ct)).ToArray());
}