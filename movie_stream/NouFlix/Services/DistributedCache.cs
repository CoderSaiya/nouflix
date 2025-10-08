using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using NouFlix.Services.Interface;

namespace NouFlix.Services;

public class DistributedCache(IDistributedCache cache) : IAppCache
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    // chống “stampede” cục bộ (trong 1 process)
    private static readonly Dictionary<string, SemaphoreSlim> KeyLocks = new();
    private static readonly Lock KeyLocksGate = new();

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var bytes = await cache.GetAsync(key, ct);
        if (bytes is null) return default;
        var json = Decompress(bytes);
        return JsonSerializer.Deserialize<T>(json, Json);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default)
        => SetInternalAsync(key, value, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        }, ct);

    public Task SetSlidingAsync<T>(string key, T value, TimeSpan slidingTtl, CancellationToken ct = default)
        => SetInternalAsync(key, value, new DistributedCacheEntryOptions
        {
            SlidingExpiration = slidingTtl
        }, ct);

    public Task RemoveAsync(string key, CancellationToken ct = default)
        => cache.RemoveAsync(key, ct);

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan ttl,
        CancellationToken ct = default)
    {
        // thử get nhanh
        var existing = await GetAsync<T>(key, ct);
        if (existing is not null && !EqualityComparer<T>.Default.Equals(existing, default!))
            return existing;

        // lock theo key (cục bộ)
        var sem = GetLock(key);
        await sem.WaitAsync(ct);
        try
        {
            // double-check sau khi vào critical section
            existing = await GetAsync<T>(key, ct);
            if (existing is not null && !EqualityComparer<T>.Default.Equals(existing, default!))
                return existing;

            // build dữ liệu
            var created = await factory(ct);
            await SetAsync(key, created, ttl, ct);
            return created;
        }
        finally
        {
            sem.Release();
        }
    }

    public Task<T> GetOrCreateSlidingAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan slidingTtl,
        CancellationToken ct = default)
        => GetOrCreateInternalSliding(key, factory, slidingTtl, ct);

    private async Task SetInternalAsync<T>(string key, T value, DistributedCacheEntryOptions opt, CancellationToken ct)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(value, Json);
        var bytes = Compress(json);
        await cache.SetAsync(key, bytes, opt, ct);
    }

    private async Task<T> GetOrCreateInternalSliding<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan slidingTtl,
        CancellationToken ct)
    {
        var existing = await GetAsync<T>(key, ct);
        if (existing is not null && !EqualityComparer<T>.Default.Equals(existing, default!))
            return existing;

        var sem = GetLock(key);
        await sem.WaitAsync(ct);
        try
        {
            existing = await GetAsync<T>(key, ct);
            if (existing is not null && !EqualityComparer<T>.Default.Equals(existing, default!))
                return existing;

            var created = await factory(ct);
            await SetSlidingAsync(key, created, slidingTtl, ct);
            return created;
        }
        finally
        {
            sem.Release();
        }
    }

    private static SemaphoreSlim GetLock(string key)
    {
        lock (KeyLocksGate)
        {
            if (!KeyLocks.TryGetValue(key, out var sem))
            {
                sem = new SemaphoreSlim(1, 1);
                KeyLocks[key] = sem;
            }
            return sem;
        }
    }

    private static byte[] Compress(ReadOnlySpan<byte> data)
    {
        using var ms = new MemoryStream();
        using (var gz = new GZipStream(ms, CompressionLevel.Fastest, leaveOpen: true))
            gz.Write(data);
        return ms.ToArray();
    }

    private static string Decompress(byte[] bytes)
    {
        using var input = new MemoryStream(bytes);
        using var gz = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gz.CopyTo(output);
        return Encoding.UTF8.GetString(output.ToArray());
    }
}