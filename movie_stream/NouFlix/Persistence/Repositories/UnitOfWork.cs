using Microsoft.EntityFrameworkCore.Storage;
using NouFlix.Persistence.Data;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Persistence.Repositories;

public class UnitOfWork(
    AppDbContext db,
    IMovieRepository movies,
    IEpisodeRepository episodes,
    IGenreRepository genres,
    IStudioRepository studios,
    IImageAssetRepository imageAssets,
    IVideoAssetRepository videoAssets,
    ISeasonRepository seasons,
    IUserRepository users,
    IRefreshTokenRepository refreshTokens)
    : IUnitOfWork
{
    public IMovieRepository Movies { get; } = movies;
    public IEpisodeRepository Episodes { get; } = episodes;
    public IGenreRepository Genres { get; } = genres;
    public IStudioRepository Studios { get; } = studios;
    public IImageAssetRepository ImageAssets { get; } = imageAssets;
    public IVideoAssetRepository VideoAssets { get; } = videoAssets;
    public ISeasonRepository Seasons { get; } = seasons;
    public IUserRepository Users { get; } = users;
    public IRefreshTokenRepository Refreshes { get; } = refreshTokens;

    private IDbContextTransaction? _tx;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => db.Database.BeginTransactionAsync(ct);

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_tx != null) await _tx.CommitAsync(ct);
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_tx != null) await _tx.RollbackAsync(ct);
    }

    public ValueTask DisposeAsync() => db.DisposeAsync();
}