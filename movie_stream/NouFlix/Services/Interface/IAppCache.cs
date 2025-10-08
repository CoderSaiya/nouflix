namespace NouFlix.Services.Interface;

public interface IAppCache
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default);
    Task SetSlidingAsync<T>(string key, T value, TimeSpan slidingTtl, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan ttl,
        CancellationToken ct = default);

    Task<T> GetOrCreateSlidingAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan slidingTtl,
        CancellationToken ct = default);
}