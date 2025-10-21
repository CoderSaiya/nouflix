using NouFlix.DTOs;

namespace NouFlix.Services.Interface;

public interface IQueue<TJob>
{
    ValueTask EnqueueAsync(TJob job, CancellationToken ct = default);
    IAsyncEnumerable<TJob> DequeueAllAsync(CancellationToken ct);
}