using System.Reactive.Linq;
using Microsoft.Extensions.Options;
using Minio;
using Minio.ApiEndpoints;
using Minio.DataModel;
using Minio.DataModel.Args;
using MoviePortal.Models.DTOs;
using MoviePortal.Models.Specification;
using NouFlix.Models.Specification;

namespace MoviePortal.Services;

public class MinioObjectStorage
{
    private readonly IMinioClient _client;
    private readonly S3Settings _cfg;

    public MinioObjectStorage(IOptions<StorageOptions> options)
    {
        _cfg = options.Value.S3;
        
        Console.Write("Endpoint minio: " + _cfg.Endpoint);

        var b = new MinioClient()
            .WithEndpoint(_cfg.Endpoint)
            .WithCredentials(_cfg.AccessKey, _cfg.SecretKey);

        if (_cfg.UseSSL) b = b.WithSSL();
        if (!string.IsNullOrWhiteSpace(_cfg.Region)) b = b.WithRegion(_cfg.Region);

        _client = b.Build();
    }

    public async Task<UploadResult> UploadAsync(string bucket, Stream stream, string objectName, string? contentType, CancellationToken ct = default)
    {
        // ensure bucket
        bool found = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket), ct);
        Console.Write("is bucket found: " + found.ToString());
        if (!found) await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket), ct);

        Stream uploadStream = stream;
        long size;

        if (!stream.CanSeek)
        {
            var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct);
            ms.Position = 0;
            uploadStream = ms;
            size = ms.Length;
        }
        else
        {
            if (stream.Position != 0) stream.Position = 0;
            size = stream.Length;
        }
        // upload
        var put = new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectName)
            .WithStreamData(uploadStream)
            .WithObjectSize(size)
            .WithContentType(contentType ?? "application/octet-stream");

        await _client.PutObjectAsync(put, ct);

        // ETag chưa có API trả trực tiếp → nếu cần, gọi StatObject
        var stat = await _client.StatObjectAsync(new StatObjectArgs().WithBucket(bucket).WithObject(objectName), ct);

        return new UploadResult(bucket, objectName, contentType, stat.Size, stat.ETag);
    }

    public Task DeleteAsync(string bucket, string objectKey, CancellationToken ct = default)
        => _client.RemoveObjectAsync(new RemoveObjectArgs().WithBucket(bucket).WithObject(objectKey), ct);

    public async Task<Uri> GetReadSignedUrlAsync(string bucket, string objectKey, TimeSpan ttl, CancellationToken ct = default)
    {
        var url = await _client.PresignedGetObjectAsync(
            new PresignedGetObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectKey)
                .WithExpiry((int)ttl.TotalSeconds));
        return new Uri(url);
    }
    
    public async Task<Uri> GetWriteSignedUrlAsync(string bucket, string objectKey, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var seconds = (int)(ttl?.TotalSeconds ?? _cfg.DefaultPresignSeconds);
        var url = await _client.PresignedPutObjectAsync(
            new PresignedPutObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectKey)
                .WithExpiry(seconds));
        return new Uri(url);
    }
    
    public async Task<(bool Ok, string? Error)> CheckBucketAsync(
        string bucket,
        bool createIfMissing = false,
        CancellationToken ct = default)
    {
        try
        {
            var exists = await _client.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucket), ct);

            if (!exists)
            {
                if (!createIfMissing)
                    return (false, "BucketNotFound");

                await _client.MakeBucketAsync(
                    new MakeBucketArgs().WithBucket(bucket), ct);

                // xác nhận lại
                exists = await _client.BucketExistsAsync(
                    new BucketExistsArgs().WithBucket(bucket), ct);

                if (!exists) return (false, "BucketCreateFailed");
            }
            
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
    
    // public async Task StreamToHttpAsync(HttpResponse http, string bucket, string objectKey, string contentType, CancellationToken ct = default)
    // {
    //     http.ContentType = contentType;
    //     await _client.GetObjectAsync(
    //         new GetObjectArgs().WithBucket(bucket).WithObject(objectKey)
    //             .WithCallbackStream(async s => { await s.CopyToAsync(http.Body, ct); }), ct);
    // }
    //
    
    public async Task<string> DownloadTextAsync(string bucket, string objectKey, CancellationToken ct = default)
    {
        using var ms = new MemoryStream();
        
        await _client.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(bucket)
                .WithObject(objectKey)
                .WithCallbackStream(s => s.CopyTo(ms)), ct);
        
        ms.Position = 0;
        return System.Text.Encoding.UTF8.GetString(ms.ToArray());
    }

    public async Task UploadTextAsync(string bucket, string objectKey, string text, string contentType, CancellationToken ct = default)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        using var ms = new MemoryStream(bytes);
        await UploadAsync(bucket, ms, objectKey, contentType, ct);
    }
}