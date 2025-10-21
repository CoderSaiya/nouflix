using Microsoft.EntityFrameworkCore.Storage;

namespace NouFlix.Persistence.Repositories.Interfaces;

public interface IUnitOfWork
{
    IMovieRepository Movies { get; }
    IEpisodeRepository Episodes { get; }
    IGenreRepository Genres { get; }
    IStudioRepository Studios { get; }
    IImageAssetRepository ImageAssets { get; }
    IVideoAssetRepository VideoAssets { get; }
    ISubtitleRepository SubtitleAssets { get; }
    ISeasonRepository Seasons { get; }
    IUserRepository Users { get; }
    IRefreshTokenRepository Refreshes { get; }
    

    Task<int> SaveChangesAsync(CancellationToken ct = default);

    // giao dịch
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}