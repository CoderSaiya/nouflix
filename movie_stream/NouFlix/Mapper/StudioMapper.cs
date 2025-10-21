using NouFlix.DTOs;
using NouFlix.Models.Entities;

namespace NouFlix.Mapper;

public static class StudioMapper
{
    public static Task<StudioRes> ToStudioResAsync(
        this Studio g,
        CancellationToken ct = default)
        => Task.FromResult(new StudioRes(g.Id, g.Name));

    public static Task<StudioRes[]> ToStudioResListAsync(
        this IEnumerable<Studio> studios,
        CancellationToken ct = default)
        => Task.WhenAll(studios.Select(s => ToStudioResAsync(s, ct)).ToArray());
}