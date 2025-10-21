using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using NouFlix.DTOs;
using NouFlix.Models.Specification;
using NouFlix.Models.ValueObject;
using NouFlix.Persistence.Repositories.Interfaces;

namespace NouFlix.Services;

public class SystemHealthService(IUnitOfWork uow, MinioObjectStorage storage, IServiceProvider sp, IOptions<StorageOptions> opts)
{
    private readonly IDistributedCache? _cache = sp.GetService<IDistributedCache>();
    private static readonly DateTime StartedUtc = DateTime.UtcNow;

    public async Task<SystemDto.SystemHealthReport> CheckAllAsync(CancellationToken ct = default)
    {
        var tasks = new List<Task<SystemDto.HealthRes>>
        {
            CheckDatabaseAsync(ct),
            CheckRedisAsync(ct),
            CheckBlobImagesAsync(ct),
            CheckBlobVideosAsync(ct)
        };

        var results = await Task.WhenAll(tasks);

        var info = new SystemDto.SystemInfo(
            Environment: Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            MachineName: Environment.MachineName,
            OSVersion: System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            Framework: System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            ProcessArchitecture: System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
            StartedAtUtc: StartedUtc,
            Uptime: DateTime.UtcNow - StartedUtc
        );

        return new SystemDto.SystemHealthReport(info, results);
    }
    
    public async Task<SystemDto.HealthRes?> CheckOneAsync(string name, CancellationToken ct = default)
        => (name.ToLowerInvariant()) switch
        {
            "db" or "database" => await CheckDatabaseAsync(ct),
            "redis" => await CheckRedisAsync(ct),
            "images" or "blobimages" => await CheckBlobImagesAsync(ct),
            "videos" or "blobvideos" => await CheckBlobVideosAsync(ct),
            _ => null
        };
    
    private async Task<SystemDto.HealthRes> CheckDatabaseAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            // Nhẹ: Any() trên Movies; nếu không có Movies thì thay bằng 1 repo khác
            var ok = await uow.Movies.Query().AsNoTracking().Select(x => x.Id).Take(1).AnyAsync(ct);
            sw.Stop();
            return new SystemDto.HealthRes(
                Name: "Database (EF Core)",
                State: HealthState.Healthy,
                Message: ok ? "Kết nối OK" : "Kết nối OK (bảng trống)",
                DurationMs: sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new SystemDto.HealthRes(
                Name: "Database (EF Core)",
                State: HealthState.Unhealthy,
                Message: ex.Message,
                DurationMs: sw.ElapsedMilliseconds);
        }
    }

    private async Task<SystemDto.HealthRes> CheckRedisAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        if (_cache is null)
        {
            return new SystemDto.HealthRes(
                Name: "Redis Cache",
                State: HealthState.NotConfigured,
                Message: "Chưa cấu hình IDistributedCache (StackExchangeRedis)",
                DurationMs: 0);
        }

        try
        {
            var key = $"health:{Guid.NewGuid():N}";
            var payload = System.Text.Encoding.UTF8.GetBytes("pong");
            var opts = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30) };

            await _cache.SetAsync(key, payload, opts, ct);
            var read = await _cache.GetAsync(key, ct);
            sw.Stop();

            if (read is not null && System.Text.Encoding.UTF8.GetString(read) == "pong")
            {
                return new SystemDto.HealthRes("Redis Cache", HealthState.Healthy, "Set/Get OK", sw.ElapsedMilliseconds);
            }
            return new SystemDto.HealthRes("Redis Cache", HealthState.Degraded, "Get trả về null/không khớp", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new SystemDto.HealthRes("Redis Cache", HealthState.Unhealthy, ex.Message, sw.ElapsedMilliseconds);
        }
    }

    private async Task<SystemDto.HealthRes> CheckBlobImagesAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var bucket = opts.Value.Buckets.Images;
        if (string.IsNullOrWhiteSpace(bucket))
            return new SystemDto.HealthRes("MinIO Bucket (Images)", HealthState.NotConfigured, "Thiếu cấu hình Storage:Bucket:Images", 0);

        try
        {
            // Thử Exists/GetProperties
            var ok = await storage.CheckBucketAsync(bucket, createIfMissing: true, ct);
            sw.Stop();
            return new SystemDto.HealthRes(
                "MinIO Bucket (Images)",
                ok.Ok ? HealthState.Healthy : HealthState.Degraded,
                ok.Ok ? $"Bucket '{bucket}' OK" : $"Bucket '{bucket}' không tồn tại/không truy cập được",
                sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new SystemDto.HealthRes("MinIO Bucket (Images)", HealthState.Unhealthy, ex.Message, sw.ElapsedMilliseconds);
        }
    }

    private async Task<SystemDto.HealthRes> CheckBlobVideosAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var bucket = opts.Value.Buckets.Videos;
        if (string.IsNullOrWhiteSpace(bucket))
            return new SystemDto.HealthRes("MinIO Bucket (Videos)", HealthState.NotConfigured, "Thiếu cấu hình AzureBlob:PrivateVideoContainer", 0);

        try
        {
            var ok = await storage.CheckBucketAsync(bucket, createIfMissing: true, ct);
            sw.Stop();
            return new SystemDto.HealthRes(
                "MinIO Bucket (Videos)",
                ok.Ok ? HealthState.Healthy : HealthState.Degraded,
                ok.Ok ? $"Bucket '{bucket}' OK" : $"Bucket '{bucket}' không tồn tại/không truy cập được",
                sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new SystemDto.HealthRes("MinIO Bucket (Videos)", HealthState.Unhealthy, ex.Message, sw.ElapsedMilliseconds);
        }
    }
}