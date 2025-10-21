using System.Collections.Concurrent;
using System.Threading.Channels;
using NouFlix.DTOs;
using NouFlix.Services.Interface;

namespace NouFlix.Services;

public class InMemoryQueue<TJob, TStatus>(Func<TStatus, string> statusKeySelector)
    : IQueue<TJob>, IStatusStorage<TStatus>
{
    private readonly ConcurrentDictionary<string, TStatus> _map = new();
    private readonly Channel<TJob> _ch =
        Channel.CreateBounded<TJob>(new BoundedChannelOptions(2) {
            FullMode = BoundedChannelFullMode.Wait, SingleReader = true, SingleWriter = false
        });
    private readonly Func<TStatus, string> _statusKeySelector = statusKeySelector 
                                                                ?? throw new ArgumentNullException(nameof(statusKeySelector));

    public ValueTask EnqueueAsync(TJob job, CancellationToken ct = default)
        => _ch.Writer.WriteAsync(job, ct);

    public IAsyncEnumerable<TJob> DequeueAllAsync(CancellationToken ct)
        => _ch.Reader.ReadAllAsync(ct);

    public void Upsert(TStatus s)
    {
        var id = _statusKeySelector(s);
        _map.AddOrUpdate(id, s, (_, __) => s);
    }
    
    public TStatus? Get(string id) => _map.GetValueOrDefault(id);
}