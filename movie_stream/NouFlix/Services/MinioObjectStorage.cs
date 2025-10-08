using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using NouFlix.DTOs;
using NouFlix.Models.Specification;

namespace NouFlix.Services;

public class MinioObjectStorage
{
    private readonly IMinioClient _client;
    private readonly S3Settings _cfg;

    public MinioObjectStorage(IOptions<StorageOptions> options)
    {
        _cfg = options.Value.S3;

        var b = new MinioClient()
            .WithEndpoint(_cfg.Endpoint)
            .WithCredentials(_cfg.AccessKey, _cfg.SecretKey);

        if (_cfg.UseSSL) b = b.WithSSL();
        if (!string.IsNullOrWhiteSpace(_cfg.Region)) b = b.WithRegion(_cfg.Region);

        _client = b.Build();
    }

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
    
    public async Task<UploadResult> UploadAsync(string bucket, Stream stream, string objectName, string? contentType, CancellationToken ct = default)
    {
        // ensure bucket
        bool found = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket), ct);
        if (!found) await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket), ct);

        // upload
        var put = new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType ?? "application/octet-stream");

        await _client.PutObjectAsync(put, ct);

        // ETag chưa có API trả trực tiếp → nếu cần, gọi StatObject
        var stat = await _client.StatObjectAsync(new StatObjectArgs().WithBucket(bucket).WithObject(objectName), ct);

        return new UploadResult(bucket, objectName, contentType, stat.Size, stat.ETag);
    }
}