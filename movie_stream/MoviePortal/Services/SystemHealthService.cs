using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MoviePortal.Models.DTOs;
using MoviePortal.Models.Specification;
using MoviePortal.Models.ValueObject;
using MoviePortal.Repositories.Interfaces;
using HealthCheckResult = MoviePortal.Models.DTOs.HealthCheckResult;

namespace MoviePortal.Services;

public class SystemHealthService(IUnitOfWork uow, MinioObjectStorage storage, IServiceProvider sp, IOptions<StorageOptions> opts)
{
    private readonly IDistributedCache? _cache = sp.GetService<IDistributedCache>(); // có thể null nếu chưa cấu hình Redis
    private static readonly DateTime StartedUtc = DateTime.UtcNow;

    // cố gắng lấy cache, nếu chưa config thì _cache=null

    public async Task<SystemHealthReport> CheckAllAsync(CancellationToken ct = default)
    {
        var tasks = new List<Task<HealthCheckResult>>
        {
            CheckDatabaseAsync(ct),
            CheckRedisAsync(ct),
            CheckBlobImagesAsync(ct),
            CheckBlobVideosAsync(ct)
        };

        var results = await Task.WhenAll(tasks);

        var info = new SystemInfo(
            Environment: Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            MachineName: Environment.MachineName,
            OSVersion: System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            Framework: System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            ProcessArchitecture: System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
            StartedAtUtc: StartedUtc,
            Uptime: DateTime.UtcNow - StartedUtc
        );

        return new SystemHealthReport(info, results);
    }

    // ---------- Checks ----------
    private async Task<HealthCheckResult> CheckDatabaseAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            // Nhẹ: Any() trên Movies; nếu không có Movies thì thay bằng 1 repo khác
            var ok = await uow.Movies.Query().AsNoTracking().Select(x => x.Id).Take(1).AnyAsync(ct);
            sw.Stop();
            return new HealthCheckResult(
                Name: "Database (EF Core)",
                State: HealthState.Healthy,
                Message: ok ? "Kết nối OK" : "Kết nối OK (bảng trống)",
                DurationMs: sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new HealthCheckResult(
                Name: "Database (EF Core)",
                State: HealthState.Unhealthy,
                Message: ex.Message,
                DurationMs: sw.ElapsedMilliseconds);
        }
    }

    private async Task<HealthCheckResult> CheckRedisAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        if (_cache is null)
        {
            return new HealthCheckResult(
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
                return new HealthCheckResult("Redis Cache", HealthState.Healthy, "Set/Get OK", sw.ElapsedMilliseconds);
            }
            return new HealthCheckResult("Redis Cache", HealthState.Degraded, "Get trả về null/không khớp", sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new HealthCheckResult("Redis Cache", HealthState.Unhealthy, ex.Message, sw.ElapsedMilliseconds);
        }
    }

    private async Task<HealthCheckResult> CheckBlobImagesAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var bucket = opts.Value.Buckets.Images;
        if (string.IsNullOrWhiteSpace(bucket))
            return new HealthCheckResult("MinIO Bucket (Images)", HealthState.NotConfigured, "Thiếu cấu hình Storage:Bucket:Images", 0);

        try
        {
            // Thử Exists/GetProperties
            var ok = await storage.CheckBucketAsync(bucket, createIfMissing: true, ct);
            sw.Stop();
            return new HealthCheckResult(
                "MinIO Bucket (Images)",
                ok.Ok ? HealthState.Healthy : HealthState.Degraded,
                ok.Ok ? $"Bucket '{bucket}' OK" : $"Bucket '{bucket}' không tồn tại/không truy cập được",
                sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new HealthCheckResult("MinIO Bucket (Images)", HealthState.Unhealthy, ex.Message, sw.ElapsedMilliseconds);
        }
    }

    private async Task<HealthCheckResult> CheckBlobVideosAsync(CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var bucket = opts.Value.Buckets.Videos;
        if (string.IsNullOrWhiteSpace(bucket))
            return new HealthCheckResult("MinIO Bucket (Videos)", HealthState.NotConfigured, "Thiếu cấu hình AzureBlob:PrivateVideoContainer", 0);

        try
        {
            var ok = await storage.CheckBucketAsync(bucket, createIfMissing: true, ct);
            sw.Stop();
            return new HealthCheckResult(
                "MinIO Bucket (Videos)",
                ok.Ok ? HealthState.Healthy : HealthState.Degraded,
                ok.Ok ? $"Bucket '{bucket}' OK" : $"Bucket '{bucket}' không tồn tại/không truy cập được",
                sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new HealthCheckResult("MinIO Bucket (Videos)", HealthState.Unhealthy, ex.Message, sw.ElapsedMilliseconds);
        }
    }
}